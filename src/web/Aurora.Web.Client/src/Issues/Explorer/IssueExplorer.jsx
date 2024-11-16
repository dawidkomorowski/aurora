import { useEffect, useState } from "react";
import { IssueList } from "./IssueList";
import { IssueApiClient } from "../../ApiClients/IssueApiClient";
import { VersionFilter } from "./Filters/VersionFilter";
import { useSearchFilters } from "./Filters/useSearchFilters";
import { StatusFilter } from "./Filters/StatusFilter";

export function IssueExplorer() {
    const [searchFilters, _] = useSearchFilters();
    const [data, setData] = useState([]);

    useEffect(() => {
        const filters = {
            status: searchFilters.status,
            versionId: searchFilters.versionId
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
                    <VersionFilter />
                </div>
            </div>
            <IssueList data={data} />
        </div>
    );
}