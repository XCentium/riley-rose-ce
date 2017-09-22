// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SampleComponent.cs" company="Sitecore Corporation">
//   Copyright (c) Sitecore Corporation 1999-2017
// </copyright>
// <summary>
//   The SampleComponent
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Plugin.Xcentium.RileyRose.Shipping
{
    using Sitecore.Commerce.Core;

    /// <summary>
    /// The SampleComponent.
    /// </summary>
    public class F21RileyRosePrdComponent : Component
    {

        public bool IsHazardous { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the comment.
        /// </summary>
        public string Comment { get; set; }
    }
}