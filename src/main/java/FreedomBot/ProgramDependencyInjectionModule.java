package FreedomBot;

import com.google.inject.AbstractModule;
import com.google.inject.Singleton;

public class ProgramDependencyInjectionModule extends AbstractModule {
  @Override
  protected void configure() {
    // singletons
    bind(AppDataDirectory.class).to(AppDataDirectory_JarLocation.class).in(Singleton.class);
    bind(SharedHttpClient.class).to(SharedHttpClient_RateLimited.class).in(Singleton.class);
    bind(SecretPassword.class).to(SecretPassword_FromConsole.class).in(Singleton.class);
    
    // instances
    bind(Console.class).to(Console_SystemConsole.class);
    bind(PriceFetcher.class);
  }
}
