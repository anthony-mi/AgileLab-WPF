using AgileLab.Data;
using AgileLab.Data.MySql;
using AgileLab.Security;
using AgileLab.Services.Logging;
using AgileLab.Services.Registry;
using AgileLab.Views.Dialog;
using Autofac;
using Autofac.Builder;
using System;
using System.Windows;
using System.Windows.Threading;

namespace AgileLab
{
    public class ComponentsContainer : IDisposable
    {
        private ComponentsContainer()
        {
            Initialise();
        }

        public static ComponentsContainer Instance { get; } = new ComponentsContainer();

        private static IContainer _container;

        public static void Initialise()
        {
            if (_container != null)
            {
                throw new InvalidOperationException($"Class {nameof(ComponentsContainer)} is already initialized.");
            }

            ContainerBuilder builder = new ContainerBuilder();

            // Services
            builder.RegisterType<BCryptHasher>().As<IHasher>();
            builder.RegisterType<RegistryService>().As<IRegistryService>();
            builder.RegisterType<FileLogger>().As<ILogger>();
            builder.RegisterType<MahAppsDialogCoordinator>().As<IDialogCoordinator>();

            // Data models
            builder.RegisterInstance(new MySqlUsersDataModel()).As<IUsersDataModel>();
            builder.RegisterInstance(new MySqlTeamsDataModel()).As<ITeamsDataModel>();
            builder.RegisterInstance(new MySqlProjectsDataModel()).As<IProjectsDataModel>();
            builder.RegisterInstance(new MySqlStoriesDataModel()).As<IStoriesDataModel>();
            builder.RegisterInstance(new MySqlBacklogsDataModel()).As<IBacklogsDataModel>();
            builder.RegisterInstance(new MySqlStoryStatusesDataModel()).As<IStoryStatusesDataModel>();
            builder.RegisterInstance(new MySqlSprintsDataModel()).As<ISprintsDataModel>();

            builder.RegisterInstance(Application.Current.Dispatcher);

            _container = builder.Build();
        }

        public static T Get<T>()
        {
            if (_container == null)
            {
                throw new NullReferenceException($"{nameof(ComponentsContainer)} not initialized.");
            }

            return _container.Resolve<T>();
        }

        public void Dispose()
        {
            StaticDispose();
        }

        public static void StaticDispose()
        {
            if (_container == null)
            {
                _container.Dispose();
            }
        }
    }
}
