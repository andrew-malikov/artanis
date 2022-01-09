namespace Domain.Collections

open Domain.Filters
open Domain.Assets.AssetEntity
open Domain.Projects.ProjectEntity
open Domain.Projects.ProjectFilters
open Domain.Collections.CollectionEntity

module CollectionFilters =
    type CollectionFilters =
        { assetFilters: Asset Filter list
          projectFilters: Project Filter list }

    let applyFilters filters collection =
        let projectFilter =
            filterProject filters.projectFilters filters.assetFilters

        { collection with
              projects = filterMany [ projectFilter ] collection.projects }
