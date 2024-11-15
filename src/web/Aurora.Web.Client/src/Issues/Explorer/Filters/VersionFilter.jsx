import { useEffect, useState } from "react";
import { VersionApiClient } from "../../../ApiClients/VersionApiClient";
import { useSearchFilters } from "./useSearchFilters";

export function VersionFilter() {
    const [searchFilters, setSearchFilters] = useSearchFilters();
    const [version, setVersion] = useState({ id: searchFilters.versionId });
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
            versionId: version.id
        };
        setSearchFilters(newSearchFilters);
    }, [version]);

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

export const ShowAllVersionFilter = {
    id: -1,
    name: "Show all"
}

export const ShowUnassignedVersionFilter = {
    id: 0,
    name: "Show unassigned"
}