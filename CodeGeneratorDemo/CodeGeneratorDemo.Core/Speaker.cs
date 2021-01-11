namespace CodeGeneratorDemo.Core
{
    public abstract class Speaker
    {
        public virtual string Name => this.GetType().ToString();
    }
}
