﻿using Net.EntityFramework.CodeGenerator.Core;
using System.Text;

namespace Net.EntityFramework.CodeGenerator
{
    public enum AccessibilityLevels
    {
        None,
        Public,
        Protected,
        Internal,
        Private,
        ProtectedInternal,
        PrivateProtected
    }

    internal class ClassGeneratorOptions
    {
        public List<string> Usings { get; set; }
        public string Namespace { get; set; }
        public string Name { get; set; }
        public AccessibilityLevels Accessibility { get; set; }
        public bool IsInterface { get; set; }
        public bool IsPartiale { get; set; }
        public bool IsStatic { get; set; }

        public List<IContentCodeSegment> Contents { get; set; }
    }

    public interface ICodeBuilder
    {
        ICodeBuilder AppendLine();
        ICodeBuilder AppendLine(string? value);

        IDisposable Indent();

        string Build();
    }
    public interface IContentCodeSegment
    {
        List<string> Usings { get; }
        void Build(ICodeBuilder builder);
    }

    internal class CodeBuilder : ICodeBuilder
    {
        private int nbrIndent = 0;
        private string _indent = string.Empty;
        private StringBuilder _str = new StringBuilder();


        public ICodeBuilder AppendLine()
        {
            _str.AppendLine();
            return this;
        }
        public ICodeBuilder AppendLine(string? value)
        {
            _str.AppendLine($"{_indent}{value}");
            return this;
        }

        public string Build()
        {
            return _str.ToString();
        }

        public IDisposable Indent() => new IndentElement(this);
        private class IndentElement : IDisposable
        {
            private readonly CodeBuilder _builder;
            public IndentElement(CodeBuilder builder)
            {
                _builder = builder;
                lock (builder)
                    _builder.AddIndent();
            }
            public void Dispose()
            {
                lock (_builder)
                    _builder.RemoveIndent();
            }
        }

        private void AddIndent()
        {
            nbrIndent++;
            SetIndent();
        }

        private void RemoveIndent()
        {
            nbrIndent--;
            SetIndent();
        }
        private void SetIndent()
        {
            var s = string.Empty;
            for (int i = 0; i < nbrIndent; i++)
                s += "    ";
            _indent = s;
        }
    }
    internal class ClassGenerator
    {
        private readonly ClassGeneratorOptions _options;

        public ClassGenerator(ClassGeneratorOptions options)
        {
            _options = options;
        }


        public string Build()
        {
            var builder = new CodeBuilder();

            var usingsLines = _options.Usings ?? new List<string>();
            if (_options.Contents != null)
            {
                foreach (var content in _options.Contents)
                {
                    if (content.Usings != null)
                    {
                        usingsLines.AddRange(content.Usings);
                    }
                }
            }

            usingsLines = usingsLines.Where(usg => usg != _options.Namespace).Distinct().ToList();
            if (usingsLines.Count > 0)
            {
                foreach (var usingItem in usingsLines)
                    builder.AppendLine($"using {usingItem};");
                builder.AppendLine();
            }
            if (!string.IsNullOrEmpty(_options.Namespace))
            {
                builder.AppendLine($"namespace {_options.Namespace}");
                builder.AppendLine("{");
            }

            using (builder.Indent())
            {
                builder.AppendLine($"{GetAccessibilit(_options.Accessibility)}{GetTypeContract(_options)} {_options.Name}");
                builder.AppendLine("{");

                if (_options.Contents != null)
                {
                    using (builder.Indent())
                    {
                        foreach (var content in _options.Contents)
                        {
                            content.Build(builder);
                        }
                    }
                }

                builder.AppendLine("}");
            }

            if (!string.IsNullOrEmpty(_options.Namespace))
                builder.AppendLine("}");
            return builder.Build();
        }

        private string GetTypeContract(ClassGeneratorOptions options)
        {
            string partial = options.IsPartiale ? "partial " : string.Empty;
            string sttc = options.IsStatic ? "static " : string.Empty;
            if (options.IsInterface)
                return $"{partial}interface";
            else
                return $"{sttc}{partial}class";
        }

        private string GetAccessibilit(AccessibilityLevels accessibility)
        {
            switch (accessibility)
            {
                case AccessibilityLevels.Public:
                    return "public ";
                case AccessibilityLevels.Protected:
                    return "protected ";
                case AccessibilityLevels.Internal:
                    return "internal ";
                case AccessibilityLevels.Private:
                    return "private ";
                case AccessibilityLevels.ProtectedInternal:
                    return "protected internal ";
                case AccessibilityLevels.PrivateProtected:
                    return "private protected ";
                default:
                case AccessibilityLevels.None:
                    return string.Empty;
            }
        }
    }
}
