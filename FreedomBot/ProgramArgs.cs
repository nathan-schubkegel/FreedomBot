namespace FreedomBot;

public class ProgramArgs
{
  private string[] _args;
  
  public ProgramArgs(string[] args)
  {
    _args = args;
  }
  
  public string this[int index] => index < _args.Length ? _args[index] : string.Empty;
  
  public int Length => _args.Length;
  
  public ProgramArgs Skip(int count) => new ProgramArgs(_args.Skip(count).ToArray());
}