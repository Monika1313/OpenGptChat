﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using OpenGptChat.Abstraction;
using OpenGptChat.Models;
using OpenGptChat.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace OpenGptChat.Services
{
    public class ApplicationHostService : IHostedService
    {
        public ApplicationHostService(
            IServiceProvider serviceProvider,
            LanguageService languageService,
            ChatStorageService chatStorageService,
            ConfigurationService configurationService)
        {
            ServiceProvider = serviceProvider;
            LanguageService = languageService;
            ChatStorageService = chatStorageService;
            ConfigurationService = configurationService;
        }

        public IServiceProvider ServiceProvider { get; }
        public LanguageService LanguageService { get; }
        public ChatStorageService ChatStorageService { get; }
        public ConfigurationService ConfigurationService { get; }



        public Task StartAsync(CancellationToken cancellationToken)
        {
            // 如果不存在配置文件则保存一波
            if (!File.Exists(GlobalValues.JsonConfigurationFilePath))
                ConfigurationService.Save();

            // 如果配置文件里面有置顶语言, 则设置语言
            CultureInfo language = CultureInfo.CurrentCulture;
            if (!string.IsNullOrWhiteSpace(ConfigurationService.Configuration.Language))
                language = new CultureInfo(ConfigurationService.Configuration.Language);
            LanguageService.SetLanguage(language);

            // 初始化服务
            ChatStorageService.Initialize();

            // 启动主窗体
            if (!Application.Current.Windows.OfType<IAppWindow>().Any())
            {
                IAppWindow window = ServiceProvider.GetService<IAppWindow>() ?? throw new InvalidOperationException("Cannot find MainWindow service");
                window.Show();

                IMainPage mainPage = ServiceProvider.GetService<IMainPage>() ?? throw new InvalidOperationException("Cannot find MainPage service");

                // 导航到主页
                window.Navigate(mainPage);
            }

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            // 关闭存储服务
            ChatStorageService.Dispose();

            return Task.CompletedTask;
        }
    }
}