using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using PhoneApp.Domain.Attributes;
using PhoneApp.Domain.DTO;
using PhoneApp.Domain.Interfaces;

namespace UserLoaderPlugin
{
    [Author(Name = "Vlad Emelyanov")]
    public class Plugin : IPluggable
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private static readonly HttpClient client = new HttpClient();

        public IEnumerable<DataTransferObject> Run(IEnumerable<DataTransferObject> args)
        {
            logger.Info("Loading users from API");
            List<EmployeesDTO> employeesList;

            try
            {
                employeesList = LoadUsersFromApi();
                logger.Info($"Loaded {employeesList.Count} users");
            }
            catch (Exception ex)
            {
                logger.Error($"An error occurred while loading users: {ex.Message}");
                return Enumerable.Empty<DataTransferObject>(); 
            }

            return employeesList.Cast<DataTransferObject>();
        }

        private List<EmployeesDTO> LoadUsersFromApi()
        {
            HttpResponseMessage response = client.GetAsync("https://dummyjson.com/users").Result;// так как задача небольшая , решил что могу позволить использовать .Result

            if (!response.IsSuccessStatusCode)
            {
                logger.Error($"Failed to load users. Status Code: {response.StatusCode}");
                return new List<EmployeesDTO>();
            }

            string responseBody = response.Content.ReadAsStringAsync().Result;
            var json = JObject.Parse(responseBody);
            List<EmployeesDTO> employees = new List<EmployeesDTO>();

            foreach (var user in json["users"])
            {
                var employee = new EmployeesDTO
                {
                    Name = user["firstName"].ToString()
                };

                var phone = user["phone"]?.ToString(); 
                if (string.IsNullOrEmpty(phone))
                {
                    logger.Warn($"Phone number is missing for user: {employee.Name}");
                    phone = "-"; 
                }

                employee.AddPhone(phone);
                employees.Add(employee);
            }

            return employees;
        }

    }
}
