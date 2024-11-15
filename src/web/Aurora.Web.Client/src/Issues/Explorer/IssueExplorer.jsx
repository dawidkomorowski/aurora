import { useEffect, useState } from "react";
import { IssueList } from "./IssueList";
import { IssueApiClient } from "../../ApiClients/IssueApiClient";
import { VersionFilter } from "./VersionFilter";
import { useSearchFilters } from "./useSearchFilters";
import { StatusFilter } from "./StatusFilter";

// TODO Refactor VersionFilter in a way that externally it operates only on a value that can be persisted in URL i.e. version ID.
// TODO When value from search params is not available in filter it should be set to some default value i.e. nonexistent version ID 123 is found in params but such version ID is not available in filters.

export function IssueExplorer() {
    const [searchFilters, setSearchFilters] = useSearchFilters();
    const [versionFilter, setVersionFilter] = useState({ id: searchFilters.versionId });
    const [data, setData] = useState([]);

    useEffect(() => {
        const newSearchFilters = {
            ...searchFilters,
            versionId: versionFilter.id
        };
        setSearchFilters(newSearchFilters);
    }, [versionFilter]);

    useEffect(() => {
        const filters = {
            status: searchFilters.status || null,
            versionId: searchFilters.versionId === -1 ? null : searchFilters.versionId
        };

        IssueApiClient.getAll(filters).then(responseData => {
            setData(responseData);
        }).catch(error => {
            console.error(error)
        });
    }, [searchFilters]);

    return (
        <div>
            <div style={{ display: "flex" }}>
                <div style={{ display: "flex" }}>
                    <div style={{ margin: "10px" }}>
                        <strong>Status</strong>
                    </div>
                    <StatusFilter />
                </div>
                <div style={{ display: "flex" }}>
                    <div style={{ margin: "10px" }}>
                        <strong>Version</strong>
                    </div>
                    <VersionFilter versionFilter={versionFilter} setVersionFilter={setVersionFilter} />
                </div>
            </div>
            <IssueList data={data} />
        </div>
    );
}