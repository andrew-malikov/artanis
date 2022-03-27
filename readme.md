# Artanis

![example workflow](https://github.com/VallanDeMorty/artanis/actions/workflows/tests.yml/badge.svg)

A CLI to download assets from [Artstation](https://artstation.com/)

## Prerequisites

To download a collection of assets you have to know the `CollectionId` and the `Username` who has the collection.

There is no official API to get it, in this way you can use your collections instead.

For example, there is a collection link `https://www.artstation.com/es-andrew/collections/1516043` where `es-andrew` is the `Username` and `1516043` is the `CollectionId`.

## Basic Usage

To just download image assets you have to specify an additional argument `-q "type=image"`

```bash
artanis collection-assets 1516043 "es-andrew" "./artstation" -q "type=image" 
```

## Corner Cases

There is no error handling under the hood, you are on your own right now

## License

MIT License
