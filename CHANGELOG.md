# Changelog

All notable changes to this repository will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/) and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## Unreleased

### Upgrade Notes

### Known Issues
* [AIRO-1606](https://jira.unity3d.com/browse/AIRO-1606) WarehouseManager GUI in PlayMode does not work
* [AIRO-1607](https://jira.unity3d.com/browse/AIRO-1607) Documentation is out of date
* [AIRO-1605](https://jira.unity3d.com/browse/AIRO-1605) Assets are imported with wrong coordinate frame in URP version
* [AIRO-1602](https://jira.unity3d.com/browse/AIRO-1602) WarehouseManager loses reference to previously created warehouse when entering/exiting PlayMode


### Added
* HDRP version of package

### Changed
* URP version has been re-named to indicate is it specifically URP

### Deprecated

### Removed
* WarehouseManager GUI deleted from scenes but not yet removed from code

### Fixed
* Scaling issues in HDRP version of project (meshes are converted from 1cm/u to 1m/u on import)
