using Core;
using DefaultNamespace;
using VContainer;
using VContainer.Unity;


public class GameLifetimeScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    { 
        builder.Register<ActionManager>(Lifetime.Singleton);
        builder.Register<GameInitializer>(Lifetime.Singleton);
        builder.Register<PlayerInventory>(Lifetime.Singleton);
        builder.RegisterEntryPoint<GameInitializer>();
    }
} 

