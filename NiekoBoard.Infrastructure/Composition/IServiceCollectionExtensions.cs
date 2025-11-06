using Markdig.Extensions.MediaLinks;
using NiekoBoard.Composition;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class IServiceCollectionExtensions
    {
        private static IDictionary<WeakReference, IDictionary<Type, object>> _CollectionPendingRegistrations = new Dictionary<WeakReference, IDictionary<Type, object>>();
        private static IDictionary<WeakReference, ISet<Type>> _CollectionCompletedRegistrations = new Dictionary<WeakReference, ISet<Type>>();

        private static WeakReference? GetCollectionRef(IServiceCollection collection)
        {
            return _CollectionPendingRegistrations.Keys.FirstOrDefault(k => k.IsAlive && k.Target == collection);
        }

        public static IServiceCollection AddMultiple<T>(this IServiceCollection collection, Func<IServiceProvider, T> factory)
            where T : class
        {
            var collectionRef = GetCollectionRef(collection);

            if (collectionRef != null && 
                _CollectionCompletedRegistrations.ContainsKey(collectionRef) &&
                _CollectionCompletedRegistrations[collectionRef].Contains(typeof(T)))
            {
                throw new InvalidOperationException("Resulting Enumeration already registered");
            }

            if (collectionRef == null)
            {
                collectionRef = new WeakReference(collection);
            }

            IDictionary<Type, object> typeActions; 

            if(!_CollectionPendingRegistrations.ContainsKey(collectionRef))
            {
                typeActions = new Dictionary<Type, object>();
                _CollectionPendingRegistrations[collectionRef] = typeActions;
                _CollectionCompletedRegistrations[collectionRef] = new HashSet<Type>();
            }
            else
            {
                typeActions = _CollectionPendingRegistrations[collectionRef];
            }

            Func<IServiceProvider, IEnumerable<T>> typeAction;

            if (typeActions.ContainsKey(typeof(T)))
            {
                var oldTypeAction = (Func<IServiceProvider, IEnumerable<T>>)typeActions[typeof(T)];

                typeAction = sp => oldTypeAction(sp)
                    .Union(new[] { factory(sp) })
                    .ToList();
            }
            else
            {
                typeAction = sp =>
                {
                    _CollectionCompletedRegistrations[collectionRef].Add(typeof(T));

                    return new[] { factory(sp) };
                };
                collection.AddTransient<IEnumerable<T>>(o => ((Func<IServiceProvider, IEnumerable<T>>)typeActions[typeof(T)])(o));
            }

            typeActions[typeof(T)] = typeAction;

            return collection;
        }
    
        public static IServiceCollection AddMultiple<T, TImplementation>(this IServiceCollection collection)
            where T: class
            where TImplementation : class, T
        {
            collection.AddTransient<TImplementation>();
            collection.AddMultiple<T>(sp => sp.GetRequiredService<TImplementation>());

            return collection;
        }
    
        public static IServiceCollection AddModules(this IServiceCollection collection, IEnumerable<IModule> modules)
        {
            foreach(var module in modules)
            {
                module.Register(collection);

                if (module is ILibraryModule)
                {
                    foreach (var libraryObject in ((ILibraryModule)module).GetLibraryObjects())
                    {
                        collection.AddMultiple<Func<ILibraryObject>>((sp) => () =>
                        {
                            var definition = libraryObject.Define();
                            definition.Initialize(sp.GetRequiredService(definition.ImplementationType));

                            return libraryObject;
                        });
                    }
                }
            }

            return collection;
        }
    }
}

