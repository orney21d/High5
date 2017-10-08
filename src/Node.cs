#region Copyright (c) 2017 Atif Aziz, Adrian Guerra
//
// Portions Copyright (c) 2013 Ivan Nikulin
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//
#endregion

namespace ParseFive
{
    using System;
    using System.Collections.Generic;
    using Extensions;
    using Microsoft.Extensions.Internal;

    public abstract class Node
    {
        public Node ParentNode { get; internal set; }
        public List<Node> ChildNodes { get; } = new List<Node>();
    }

    public class Document : Node
    {
        public string Mode { get; internal set; }
    }

    public class DocumentFragment : Node {}

    public class DocumentType : Node
    {
        public string Name     { get; internal set; }
        public string PublicId { get; internal set; }
        public string SystemId { get; internal set; }

        public DocumentType(string name, string publicId, string systemId)
        {
            Name = name;
            PublicId = publicId;
            SystemId = systemId;
        }
    }

    public struct Attribute : IEquatable<Attribute>
    {
        public string NamespaceUri { get; }
        public string Prefix       { get; }
        public string Name         { get; }
        public string Value        { get; }

        public Attribute(string name, string value) :
            this(null, null, name, value) {}

        public Attribute(string namespaceUri, string prefix, string name, string value)
        {
            NamespaceUri = namespaceUri;
            Prefix       = prefix;
            Name         = name;
            Value        = value;
        }

        public bool Equals(Attribute other) =>
            string.Equals(NamespaceUri, other.NamespaceUri)
            && string.Equals(Prefix, other.Prefix, StringComparison.OrdinalIgnoreCase)
            && string.Equals(Name, other.Name, StringComparison.OrdinalIgnoreCase)
            && string.Equals(Value, other.Value);

        public override bool Equals(object obj) =>
            obj is Attribute attribute && Equals(attribute);

        public override int GetHashCode()
        {
            var hashCode = HashCodeCombiner.Start();
            hashCode.Add(NamespaceUri);
            hashCode.Add(Prefix, StringComparer.OrdinalIgnoreCase);
            hashCode.Add(Name, StringComparer.OrdinalIgnoreCase);
            hashCode.Add(Value);
            return hashCode;
        }
    }

    public class Element : Node
    {
        IList<Attribute> _attrs;

        public string TagName { get; }
        public string NamespaceUri { get; }

        static readonly Attribute[] ZeroAttrs = new Attribute[0];

        public IList<Attribute> Attributes
        {
            get => _attrs ?? ZeroAttrs;
            private set => _attrs = value;
        }

        public Element(string tagName, string namespaceUri, IList<Attribute> attributes)
        {
            TagName = tagName;
            NamespaceUri = namespaceUri;
            Attributes = attributes;
        }

        internal void AttributesPush(Attribute attr)
        {
            // TODO remove ugly hack
            if (_attrs is null || _attrs is Attribute[] a && a.Length == 0)
                _attrs = new List<Attribute>();
            _attrs.Push(attr);
        }
    }

    public class TemplateElement : Element
    {
        public Node Content { get; internal set; }

        public TemplateElement(string tagName, string namespaceUri, IList<Attribute> attributes) :
            base(tagName, namespaceUri, attributes) {}
    }

    public class Comment : Node
    {
        public string Data { get; }
        public Comment(string data) => Data = data;
    }

    public class Text : Node
    {
        public string Value { get; internal set; }
        public Text(string value) => Value = value;
    }
}