import { useEffect, useState } from "react";
import { VersionApiClient } from "../../../ApiClients/VersionApiClient";
import { useSearchFilters } from "./useSearchFilters";

// TODO When value from search params is not available in filter it should be set to some default value i.e. nonexistent version ID 123 is found in params but such version ID is not available in filters.
// TODO Challenge is in fact that once the page is refreshed and before fetching filter options it will always fallback to default value.

export function VersionFilter() {
    const [searchFilters, setSearchFilters] = useSearchFilters();
    const [version, setVersion] = useState(ShowAllVersionFilter);
    const [versions, setVersions] = useState([]);

    const versionsNotYetLoaded = versions.length === 0;

    useEffect(() => {
        VersionApiClient.getAll().then(responseData => {
            setVersions([ShowAllVersionFilter, ShowUnassignedVersionFilter, ...responseData]);
        }).catch(error => {
            console.error(error)
        });
    }, []);

    useEffect(() => {
        if (versionsNotYetLoaded) {
            return;
        }

        const newSearchFilters = {
            ...searchFilters,
            versionId: version === ShowAllVersionFilter ? null : version.id
        };

        setSearchFilters(newSearchFilters);
    }, [version]);

    useEffect(() => {
        if (versionsNotYetLoaded) {
            return;
        }

        if (version === ShowAllVersionFilter && searchFilters.versionId === null) {
            return;
        }

        if (version.id === searchFilters.versionId) {
            return;
        }

        const versionFromSearchFilters = versions.find(v => v.id === (searchFilters.versionId ?? -1)) ?? ShowAllVersionFilter;
        setVersion(versionFromSearchFilters);
    }, [searchFilters, versions]);

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