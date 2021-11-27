namespace Application.Filters

open Application.Entities

module CollectionFilters =
    let filterCollection projectFilters (collection: Collection) =
        { collection with
              projects = projectFilters collection.projects }
