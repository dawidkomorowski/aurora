import { useState, useEffect } from "react";
import { useSearchParams } from "react-router-dom";

export function useSearchFilters() {
    const [searchParams, setSearchParams] = useSearchParams();
    const [searchFilters, setSearchFilters] = useState(toSearchFilters(searchParams));

    useEffect(() => {
        setSearchFilters(toSearchFilters(searchParams));
    }, [searchParams]);

    useEffect(() => {
        const newSearchParams = new URLSearchParams();

        if (searchFilters.status) {
            newSearchParams.set(statusFilterName, searchFilters.status);
        }

        if (searchFilters.versionId != -1) {
            newSearchParams.set(versionFilterName, searchFilters.versionId);
        }

        setSearchParams(newSearchParams);
    }, [searchFilters]);

    return [searchFilters, setSearchFilters];
}

const statusFilterName = "status";
const versionFilterName = "versionId";

function toSearchFilters(searchParams) {
    return {
        status: searchParams.get(statusFilterName) ?? "",
        versionId: searchParams.get(versionFilterName) ?? -1
    };
}