using System.IO;

namespace SearchEngine
{
    public abstract class Field
    {
        protected Field(string name, FieldFlags flags)
        {
            Name = name;
            Flags = flags;
        }

        public string Name { get; }

        public FieldFlags Flags { get; }

        public abstract StreamReader OpenReader();

        public override string ToString()
        {
            using (var reader = OpenReader())
            {
                return reader.ReadToEnd();
            }
        }
    }
}