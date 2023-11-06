using CompanyApi;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Xunit;

namespace CompanyApiTest
{
    public class CompanyApiTest
    {
        private readonly HttpClient httpClient;
        private readonly WebApplicationFactory<Program> webApplicationFactory;

        public CompanyApiTest()
        {
            webApplicationFactory = new WebApplicationFactory<Program>();
            httpClient = webApplicationFactory.CreateClient();
        }

        [Fact]
        public async Task Should_return_created_company_with_status_201_when_create_company_given_a_company_name()
        {
            // Given
            await ClearDataAsync();
            var companyGiven = new CreateCompanyRequest
            {
                Name = "BlueSky Digital Media"
            };
            
            // When
            var httpResponseMessage = await httpClient.PostAsJsonAsync("/api/companies", companyGiven);
           
            // Then
            Assert.Equal(HttpStatusCode.Created, httpResponseMessage.StatusCode);
            var companyCreated = await httpResponseMessage.Content.ReadFromJsonAsync<Company>();
            Assert.NotNull(companyCreated);
            Assert.NotNull(companyCreated.Id);
            Assert.Equal(companyGiven.Name, companyCreated.Name);
        }

        [Fact]
        public async Task Should_return_bad_reqeust_when_create_company_given_a_company_with_unknown_field()
        {
            // Given
            await ClearDataAsync();
            StringContent content = new StringContent("{\"unknownField\": \"BlueSky Digital Media\"}", Encoding.UTF8, "application/json");
          
            // When
            HttpResponseMessage httpResponseMessage = await httpClient.PostAsync("/api/companies", content);
           
            // Then
            Assert.Equal(HttpStatusCode.BadRequest, httpResponseMessage.StatusCode);
        }
        [Fact]
        public async Task Should_return_bad_request_when_create_company_given_an_existed_company_name()
        {
            // Given
            await ClearDataAsync();
            var companyGiven = new CreateCompanyRequest
            {
                Name = "BlueSky Digital Media"
            };

            // When
            await httpClient.PostAsJsonAsync("/api/companies", companyGiven);
            var httpResponseMessage = await httpClient.PostAsJsonAsync("/api/companies", companyGiven);

            // Then
            Assert.Equal(HttpStatusCode.BadRequest, httpResponseMessage.StatusCode);
        }

        [Fact]
        public async Task Should_return_all_companies_when_get_companies_given_all()
        {
            // Given
            await ClearDataAsync();
            var companyOne = new CreateCompanyRequest
            {
                Name = "BlueSky Digital Media"
            };
            var companyTwo = new CreateCompanyRequest
            {
                Name = "ThoughtWorks"
            };
            await httpClient.PostAsJsonAsync("/api/companies", companyOne);
            await httpClient.PostAsJsonAsync("/api/companies", companyTwo);

            // When
            var httpResponseMessage = await httpClient.GetAsync("/api/companies");
        
            // Then
            Assert.Equal(HttpStatusCode.OK, httpResponseMessage.StatusCode);
            var companies = await httpResponseMessage.Content.ReadFromJsonAsync<List<Company>>();
            Assert.NotNull(companies);
            Assert.Equal(2, companies.Count);
        }

        [Fact]
        public async Task Should_return_company_when_get_company_given_existed_company_id()
        {
            // Given
            await ClearDataAsync();
            var companyGiven = new CreateCompanyRequest
            {
                Name = "BlueSky Digital Media"
            };
            var httpResponseMessage = await httpClient.PostAsJsonAsync("/api/companies", companyGiven);
            var companyCreated = await httpResponseMessage.Content.ReadFromJsonAsync<Company>();
            Assert.NotNull(companyCreated);
            Assert.NotNull(companyCreated.Id);

            // When
            httpResponseMessage = await httpClient.GetAsync($"/api/companies/{companyCreated.Id}");
           
            // Then
            Assert.Equal(HttpStatusCode.OK, httpResponseMessage.StatusCode);
            var company = await httpResponseMessage.Content.ReadFromJsonAsync<Company>();
            Assert.NotNull(company);
            Assert.Equal(companyCreated.Id, company.Id);
            Assert.Equal(companyCreated.Name, company.Name);
        }

        [Fact]
        public async Task Should_return_not_found_when_get_company_given_not_existed_company_id()
        {
            // Given
            await ClearDataAsync();
            var companyGiven = new CreateCompanyRequest
            {
                Name = "BlueSky Digital Media"
            };
            var httpResponseMessage = await httpClient.PostAsJsonAsync("/api/companies", companyGiven);
            var companyCreated = await httpResponseMessage.Content.ReadFromJsonAsync<Company>();
            Assert.NotNull(companyCreated);
            Assert.NotNull(companyCreated.Id);


            // When
            httpResponseMessage = await httpClient.GetAsync($"/api/companies/{Guid.NewGuid().ToString()}");

            // Then
            Assert.Equal(HttpStatusCode.NotFound, httpResponseMessage.StatusCode);
        }

        private async Task ClearDataAsync()
        {
            await httpClient.DeleteAsync("/api/companies");
        }
    }
}
