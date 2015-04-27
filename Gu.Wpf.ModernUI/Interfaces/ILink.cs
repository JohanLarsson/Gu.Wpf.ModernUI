﻿namespace Gu.Wpf.ModernUI
{
    using System;

    /// <summary>
    /// 
    /// </summary>
    public interface ILink
    {
        /// <summary>
        /// The displayname
        /// </summary>
        string DisplayName { get; }
        
        /// <summary>
        /// The source
        /// </summary>
        Uri Source { get; }
    }
}