package FreedomBot;

import com.google.inject.AbstractModule;
import com.google.inject.Singleton;

public class ProgramDependencyInjectionModule extends AbstractModule {
  @Override
  protected void configure() {
    bind(SharedHttpClient.class).to(RateLimitedHttpClient.class).in(Singleton.class);
    bind(PriceFetcher.class);
  }
}
