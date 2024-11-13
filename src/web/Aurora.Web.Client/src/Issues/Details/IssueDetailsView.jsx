import { useParams } from "react-router-dom";
import { useState, useEffect, version } from "react";
import { IssueApiClient } from "../../ApiClients/IssueApiClient";
import { IssueDetails } from "./IssueDetails";
import { IssueEditor } from "./IssueEditor";
import { NoVersion } from "../VersionSelect";

export function IssueDetailsView() {
    const { issueId } = useParams();
    const [editMode, setEditMode] = useState(false);
    const [data, setData] = useState({});

    useEffect(() => {
        IssueApiClient.get(issueId).then(responseData => {
            setData(createDataFromResponse(responseData));
        }).catch(error => {
            console.error(error);
        });
    }, []);

    function createDataFromResponse(responseData) {
        if (responseData.version === null) {
            responseData.version = NoVersion;
        }

        return responseData;
    }

    function handleEditButtonClick() {
        setEditMode(true);
    }

    function handleSaveButtonClick() {
        const versionId = data.version.id === NoVersion.id ? null : data.version.id;
        IssueApiClient.update(data.id, data.title, data.description, data.status, versionId).then(responseData => {
            setData(createDataFromResponse(responseData));
        }).catch(error => {
            console.error(error);
        });

        setEditMode(false);
    }

    let content;
    let button;
    if (editMode) {
        content = <div style={{ marginTop: "20px" }}><IssueEditor data={data} setData={setData} /></div>
        button = <button onClick={handleSaveButtonClick}>Save</button>
    }
    else {
        content = <IssueDetails title={data.title} description={data.description} status={data.status} version={data.version} />
        button = <button onClick={handleEditButtonClick}>Edit</button>
    }

    return (
        <div style={{ display: "flex", justifyContent: "center" }}>
            <div style={{ width: "50%", backgroundColor: "lightgray", padding: "10px" }} >
                <div><strong>Id: </strong>{data.id}</div>
                {content}
                <br />
                <div><strong>Created: </strong>{data.createdDateTime}</div>
                <div><strong>Updated: </strong>{data.updatedDateTime}</div>
                <div style={{ marginTop: "20px", float: "right" }}>
                    {button}
                </div>
            </div>
        </div >
    );
}
