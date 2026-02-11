using UnityEngine;
using Hostage.Core;
using Hostage.Graphs;
using VContainer;
using VContainer.Unity;
using Hostage.SO;
using Hostage.UI;

namespace Hostage.Scopes
{
    public class GameLifetimeScope : LifetimeScope
    {
        [Header("Databases")]
        public SOPersonList personList;
        public IntelList allIntelList;
        public EventGraph firstGraph;
        [Header("UI")]
        public UIManager uiManager;
        
        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<SignalBus>(Lifetime.Singleton);
            builder.Register<ActionManager>(Lifetime.Singleton);
            builder.Register<GameInitializer>(Lifetime.Singleton);
            builder.Register<PlayerInventory>(Lifetime.Singleton);
            builder.Register<EventGraphRunner>(Lifetime.Singleton);
            builder.RegisterEntryPoint<GameInitializer>();
            builder.RegisterInstance(personList);
            builder.RegisterInstance(allIntelList);
            builder.RegisterInstance(firstGraph);
            builder.Register<PersonManager>(Lifetime.Singleton);
            builder.Register<IntelProvider>(Lifetime.Singleton);
            builder.RegisterInstance(uiManager);
        }
    }
}
