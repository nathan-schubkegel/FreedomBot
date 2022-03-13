using Xunit.Abstractions;
using Xunit.Sdk;
using System.Globalization;

// This file runs code before all tests and after all tests
// Taken from https://stackoverflow.com/a/53143426

[assembly: Xunit.TestFramework("MyNamespace.MyClassName", "MyAssemblyName")]

namespace FreedomBot.Tests;

public class GlobalSetupTeardown : XunitTestFramework
{
  public GlobalSetupTeardown(IMessageSink messageSink) : base(messageSink)
  {
    // Make it so ToString() and TryParse() never need to be given CultureInfo.InvariantCulture
    CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
    CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;
  }

  public new void Dispose()
  {
    // FUTURE: tear down code can go here
    base.Dispose();
  }
}
