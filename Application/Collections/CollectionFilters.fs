namespace Application.Collections

open Application.Filters
open Application.Collections.CollectionEntity

module CollectionFilters =
    let filterCollection projectFilters collection =
        { collection with
              projects = filterMany projectFilters collection.projects }
