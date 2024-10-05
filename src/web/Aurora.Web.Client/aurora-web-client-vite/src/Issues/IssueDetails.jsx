import { useState, useEffect } from "react";
import { IssuesServiceClient } from "./IssuesServiceClient";


export function IssueDetails() {
    const [data, setData] = useState([]);

    useEffect(() => {
        IssuesServiceClient.get(1).then(responseData => {
            setData(responseData);
        }).catch(error => {
            console.error(error);
        });
    }, []);

    return (
        <>
            <div>
                <div>Id: {data.id}</div>
                <div>Title: {data.title}</div>
                <div>Description: {data.description}</div>
                <div>Status: {data.status}</div>
                <div>Created: {data.createdDateTime}</div>
                <div>Updated: {data.updatedDateTime}</div>
            </div>
        </>
    );
}
