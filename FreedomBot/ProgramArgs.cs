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
  
  public IReadOnlyList<string> Args => _args;
  
  public int IndexOf(string value)
  {
    for (int i = 0; i < _args.Length; i++)
    {
      if (value == _args[i]) return i;
    }
    return -1;
  }
}