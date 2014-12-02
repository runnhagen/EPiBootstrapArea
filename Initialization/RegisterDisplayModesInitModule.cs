﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using EPiServer.Data;
using EPiServer.Data.Dynamic;
using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.ServiceLocation;
using EPiServer.Web;

namespace EPiBootstrapArea.Initialization
{
    [ModuleDependency(typeof(DataInitialization))]
    [ModuleDependency(typeof(DisplayModeFallbackProviderInitModule))]
    public class RegisterDisplayModesInitModule : IInitializableModule
    {
        private DynamicDataStore _store;

        private DynamicDataStore Store
        {
            get
            {
                return _store ?? (_store = typeof(DisplayModeFallback).GetStore());
            }
        }

        public void Initialize(InitializationEngine context)
        {
            RegisterModesInDynamicStore();
            RegisterDisplayOptions();
        }

        public void Uninitialize(InitializationEngine context)
        {
        }

        public void Preload(string[] parameters)
        {
        }

        private void RegisterDisplayOptions()
        {
            var options = ServiceLocator.Current.GetInstance<DisplayOptions>();
            var modes = Store.LoadAll<DisplayModeFallback>().ToList();
            Debug.WriteLine("Number " + modes.Count());

            foreach (var mode in modes)
            {
                options.Add(new DisplayOption
                {
                    Id = mode.Tag,
                    Name = "/displayoptions/" + mode.Tag,
                    Tag = mode.Tag,
                    IconClass = mode.Icon
                });
            }
        }

        /// <summary>
        ///     Registers display modes fallbacks in DDS - will replace items with existing key, and only register the las
        /// </summary>
        private void RegisterModesInDynamicStore()
        {
            Store.DeleteAll();
            var initialData = ServiceLocator.Current.GetInstance<IDisplayModeFallbackProvider>().GetAll();
            ValidateInitialData(initialData);

            foreach (var mode in initialData)
            {
                Store.Save(mode);
            }
        }

        private static void ValidateInitialData(IEnumerable<DisplayModeFallback> initialData)
        {
            var duplicateTagsRegistered = initialData.GroupBy(x => x.Tag)
                                                     .Select(g => new { Value = g.Key, Count = g.Count() })
                                                     .OrderByDescending(x => x.Count);

            foreach (var tagGroup in duplicateTagsRegistered.Where(tagGroup => tagGroup.Count > 1))
            {
                throw new ArgumentException("Multiple DisplayFallback options are registered with tag = " + tagGroup.Value);
            }
        }
    }
}
