import { useEffect, useState } from "react";
import { VersionApiClient } from "../../../ApiClients/VersionApiClient";
import { useSearchFilters } from "./useSearchFilters";

// TODO When value from search params is not available in filter it should be set to some default value i.e. nonexistent version ID 123 is found in params but such version ID is not available in filters.
// TODO Challenge is in fact that once the page is refreshed and before fetching filter options it will always fallback to default value.

export function VersionFilter() {
    const [searchFilters, setSearchFilters] = useSearchFilters();
    const [version, setVersion] = useState(searchFilters.versionId === null ? ShowAllVersionFilter : { id: parseInt(searchFilters.versionId) ?? -1 });
    const [versions, setVersions] = useState([]);

    useEffect(() => {
        VersionApiClient.getAll().then(responseData => {
            setVersions([ShowAllVersionFilter, ShowUnassignedVersionFilter, ...responseData]);
        }).catch(error => {
            console.error(error)
        });
    }, []);

    useEffect(() => {
        const newSearchFilters = {
            ...searchFilters,
            versionId: version === ShowAllVersionFilter ? null : version.id
        };
        setSearchFilters(newSearchFilters);
    }, [version]);

    useEffect(() => {
        const versionId = searchFilters.versionId === null ? -1 : parseInt(searchFilters.versionId);
        const versionFromSearchFilters = versions.find(v => v.id === versionId) ?? ShowAllVersionFilter;
        setVersion(versionFromSearchFilters);
    }, [searchFilters]);

    function handleInput(event) {
        const versionId = parseInt(event.target.value);
        const selectedVersion = versions.find(v => v.id === versionId);
        setVersion(selectedVersion);
    }

    const options = versions.map(v => <option key={v.id} value={v.id}>{v.name}</option>);

    return (
        <select value={version.id} onInput={handleInput} style={{ width: "200px" }}>
            {options}
        </select>
    );
}

const ShowAllVersionFilter = {
    id: -1,
    name: "Show all"
}

const ShowUnassignedVersionFilter = {
    id: 0,
    name: "Show unassigned"
}