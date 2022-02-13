namespace Domain.Projects

open System

open Domain.Assets.AssetEntity

module ProjectEntity =
    type Category = { name: string; id: int }
    
    type Project =
        { assets: Asset list
          categories: Category list
          createdAt: DateTime
          description: string
          hashId: string
          id: int
          permalink: string
          publishedAt: DateTime
          tags: string list
          title: string
          updatedAt: DateTime
          userId: int }
