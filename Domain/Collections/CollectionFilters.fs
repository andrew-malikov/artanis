namespace Domain.Collections

open Domain.Filters
open Domain.Collections.CollectionEntity

module CollectionFilters =
    let filterCollection projectFilters collection =
        { collection with
              projects = filterMany projectFilters collection.projects }
