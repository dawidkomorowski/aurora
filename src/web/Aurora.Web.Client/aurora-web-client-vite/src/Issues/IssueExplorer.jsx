import { useEffect, useState } from "react";
import { IssueList } from "./IssueList";
import { IssuesServiceClient } from "./IssuesServiceClient"

export function IssueExplorer() {
    const [data, setData] = useState([]);

    useEffect(() => {
        IssuesServiceClient.getAll().then(responseData => {
            setData(responseData);
        }).catch(error => {
            console.error(error)
        });
    }, []);

    return (
        <>
            <IssueList data={data} />
        </>
    );
}

