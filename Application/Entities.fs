namespace Application

open System

module Entities =
    type Asset =
        { assetType: string
          hasImage: bool
          height: int
          id: int
          imageUrl: string
          position: int
          title: string option
          titleFormatted: string
          viewportConstraintType: string
          width: int }
    
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
    
    type CollectionMetadata =
        { id: int
          name: string
          projectsCount: int
          userId: int }
        
    type Collection =
        { metadata: CollectionMetadata
          projects: Project list }
