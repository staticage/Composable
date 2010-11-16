﻿using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Composable.System.Hierarchies
{
    /// <summary>
    /// Provides a minimal interface for representing a data type which 
    /// is hierarchical in the form that each instance has a collection 
    /// of other instances.
    /// 
    /// Implementing this interface gives access to all the extension methods 
    /// implemented upon it which is the main purpose of doing so.
    /// 
    /// <example>
    /// A simplistic example might look like this:
    /// <code>
    ///class Person : IHierarchy&lt;Person&gt;
    ///{
    ///    .....
    ///    private IList&lt;Person&gt; _children = new List&lt;Person&gt;();
    ///    public IEnumerable&lt;Person&gt; Children { get { return _children; } }
    ///}
    /// </code>
    /// </example>
    /// 
    /// 
    /// An alternative to implementing this interface is to use <see cref="HierarchyExtensions.AsHierarchy{T}"/>
    /// </summary>
    [ContractClass(typeof(HierarchyContract<>))]
    public interface IHierarchy<T> where T : IHierarchy<T>
    {
        /// <summary>
        /// Returns the collection direct descendants of this node.
        /// </summary>
        IEnumerable<T> Children { get; }
    }

    [ContractClassFor(typeof(IHierarchy<>))]
    internal abstract class HierarchyContract<T> : IHierarchy<T> where T : IHierarchy<T>
    {
        public IEnumerable<T> Children { get
        {
            Contract.Ensures(Contract.Result<IEnumerable<HierarchyContract<T>>>() != null);
            throw new NotImplementedException();
        } }
    }
}