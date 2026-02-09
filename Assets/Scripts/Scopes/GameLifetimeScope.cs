using UnityEngine;
using Hostage.Core;
using VContainer;
using VContainer.Unity;
using Hostage.SO;

namespace Hostage.Scopes
{
    public class GameLifetimeScope : LifetimeScope
    {
        [Header("Databases")]
        public ActionPersonList personList;
        public IntelList intelList;

        protected override void Configure(IContainerBuilder builder)
        { 
            builder.Register<ActionManager>(Lifetime.Singleton);
            builder.Register<GameInitializer>(Lifetime.Singleton);
            builder.Register<PlayerInventory>(Lifetime.Singleton);
            builder.RegisterEntryPoint<GameInitializer>();
            builder.RegisterInstance(personList);
            builder.RegisterInstance(intelList);
            builder.Register<PersonProvider>(Lifetime.Singleton);
            builder.Register<IntelProvider>(Lifetime.Singleton);
        }
    }
}
