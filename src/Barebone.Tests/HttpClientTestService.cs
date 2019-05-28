﻿using Infrastructure.External.DanLirisClient.Microservice.HttpClientService;
using System.Net.Http;
using System.Threading.Tasks;

namespace Barebone.Tests
{
    public class HttpClientTestService : IHttpClientService
    {
        public static string Token;

        public Task<HttpResponseMessage> GetAsync(string url)
        {
            //return Task.Run(() => new HttpResponseMessage() { Content = new StringContent("{ 'data' : {'Name' : 'Name', 'Code' : 'Code'} }") });
            return Task.Run(() => new HttpResponseMessage() { Content = new StringContent("{}") });
        }

        public Task<HttpResponseMessage> GetAsync(string url, string token)
        {
            //return Task.Run(() => new HttpResponseMessage() { Content = new StringContent("{ 'data' : {'Name' : 'Name', 'Code' : 'Code'} }") });
            return Task.Run(() => new HttpResponseMessage() { Content = new StringContent("{}") });
        }

        public Task<HttpResponseMessage> PostAsync(string url, string token, HttpContent content)
        {
            return Task.Run(() => new HttpResponseMessage());
        }

        public Task<HttpResponseMessage> PostAsync(string url, HttpContent content)
        {
            return Task.Run(() => new HttpResponseMessage() { Content = new StringContent("{ 'data' : 'Bearer Test' }") });
        }
    }
}
