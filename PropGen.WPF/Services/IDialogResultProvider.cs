namespace PropGen.WPF.Services
{
    public interface IDialogResultProvider<TResult>
    {
        TResult GetResult();
    }
}
