namespace Application.Projects

open System

open Application.Assets.AssetEntity

module ProjectEntity =
    type Project =
        { assets: Asset List
          categories: {| name: string; id: int |} List
          createdAt: DateTime
          description: string
          hashId: string
          id: int
          permalink: string
          publishedAt: DateTime
          tags: string List
          title: string
          updatedAt: DateTime
          userId: int }
