// Copyright 2016-2019, Pulumi Corporation

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Pulumi
{
    /// <summary>
    /// A <see cref="Resource"/> that aggregates one or more other child resources into a higher
    /// level abstraction. The component resource itself is a resource, but does not require custom
    /// CRUD operations for provisioning.
    /// </summary>
    public class ComponentResource : Resource
    {
        internal readonly bool remote;

        /// <summary>
        /// Creates and registers a new component resource.  <paramref name="type"/> is the fully
        /// qualified type token and <paramref name="name"/> is the "name" part to use in creating a
        /// stable and globally unique URN for the object. <c>options.parent</c> is the optional parent
        /// for this component, and [options.dependsOn] is an optional list of other resources that
        /// this resource depends on, controlling the order in which we perform resource operations.
        /// </summary>
        /// <param name="type">The type of the resource.</param>
        /// <param name="name">The unique name of the resource.</param>
        /// <param name="options">A bag of options that control this resource's behavior.</param>
        public ComponentResource(string type, string name, ComponentResourceOptions? options = null)
            : this(type, name, ResourceArgs.Empty, options, remote: false, registerPackageRequest: null)
        {
        }

        /// <summary>
        /// Creates and registers a new component resource.  <paramref name="type"/> is the fully
        /// qualified type token and <paramref name="name"/> is the "name" part to use in creating a
        /// stable and globally unique URN for the object. <c>options.parent</c> is the optional parent
        /// for this component, and [options.dependsOn] is an optional list of other resources that
        /// this resource depends on, controlling the order in which we perform resource operations.
        /// </summary>
        /// <param name="type">The type of the resource.</param>
        /// <param name="name">The unique name of the resource.</param>
        /// <param name="args">The arguments to use to populate the new resource.</param>
        /// <param name="options">A bag of options that control this resource's behavior.</param>
        /// <param name="remote">True if this is a remote component resource.</param>
        public ComponentResource(
            string type, string name, ResourceArgs? args, ComponentResourceOptions? options = null, bool remote = false)
            : this(type, name, args ?? ResourceArgs.Empty, options ?? new ComponentResourceOptions(), remote, registerPackageRequest: null)
        {

        }

        /// <summary>
        /// Creates and registers a new component resource.  <paramref name="type"/> is the fully
        /// qualified type token and <paramref name="name"/> is the "name" part to use in creating a
        /// stable and globally unique URN for the object. <c>options.parent</c> is the optional parent
        /// for this component, and [options.dependsOn] is an optional list of other resources that
        /// this resource depends on, controlling the order in which we perform resource operations.
        /// </summary>
        /// <param name="type">The type of the resource.</param>
        /// <param name="name">The unique name of the resource.</param>
        /// <param name="args">The arguments to use to populate the new resource.</param>
        /// <param name="options">A bag of options that control this resource's behavior.</param>
        /// <param name="remote">True if this is a remote component resource.</param>
        /// <param name="registerPackageRequest">Package parameterization options.</param>
#pragma warning disable RS0022 // Constructor make noninheritable base class inheritable
        public ComponentResource(
            string type,
            string name,
            ResourceArgs? args,
            ComponentResourceOptions? options,
            bool remote,
            RegisterPackageRequest? registerPackageRequest = null)
            : base(type, name, custom: false, args ?? ResourceArgs.Empty, options ?? new ComponentResourceOptions(), remote, dependency: false, registerPackageRequest)
#pragma warning restore RS0022 // Constructor make noninheritable base class inheritable
        {
            this.remote = remote;
        }

        /// <summary>
        /// RegisterOutputs registers synthetic outputs that a component has initialized, usually by
        /// allocating other child sub-resources and propagating their resulting property values.
        /// ComponentResources should always call this at the end of their constructor to indicate
        /// that they are done creating child resources.  While not strictly necessary, this helps
        /// the experience by ensuring the UI transitions the ComponentResource to the 'complete'
        /// state as quickly as possible (instead of waiting until the entire application completes).
        /// </summary>
        protected void RegisterOutputs()
        {
            var outputs = new Dictionary<string, object?>();
            var currentType = GetType();
            foreach (var prop in currentType.GetProperties())
            {
                if (prop.Name == nameof(Urn))
                {
                    continue;
                }

                var outputAttribute = prop.GetCustomAttribute<OutputAttribute>();

                if (outputAttribute != null)
                {
                    var registerdOutputKey = prop.Name;
                    if (!string.IsNullOrWhiteSpace(outputAttribute.Name))
                    {
                        // when using [Output("<name>")] we will export the value of this property
                        // with its key equal to the provided <name>
                        registerdOutputKey = outputAttribute.Name;
                    }

                    // otherwise if we only have [Output] we will simply use the name of the property itself
                    var value = prop.GetValue(this);
                    outputs.Add(registerdOutputKey, value);
                }
            }

            RegisterOutputs(outputs);
        }

        protected void RegisterOutputs(IDictionary<string, object?> outputs)
            => RegisterOutputs(Task.FromResult(outputs ?? throw new ArgumentNullException(nameof(outputs))));

        protected void RegisterOutputs(Task<IDictionary<string, object?>> outputs)
            => RegisterOutputs(Output.Create(outputs ?? throw new ArgumentNullException(nameof(outputs))));

        protected void RegisterOutputs(Output<IDictionary<string, object?>> outputs)
            => Deployment.InternalInstance.RegisterResourceOutputs(this, outputs ?? throw new ArgumentNullException(nameof(outputs)));
    }
}
