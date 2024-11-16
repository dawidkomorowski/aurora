import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { IssueApiClient } from "../ApiClients/IssueApiClient";
import { NoVersion, VersionSelect } from "./VersionSelect";
import { AuroraTitle } from "../Components/AuroraTitle";

export function CreateIssueView() {
    const navigate = useNavigate();
    const [title, setTitle] = useState("");
    const [description, setDescription] = useState("");
    const [version, setVersion] = useState(NoVersion);

    function handleTitleInput(event) {
        setTitle(event.target.value);
    }

    function handleDescriptionInput(event) {
        setDescription(event.target.value);
    }

    function handleCreateButtonClick() {
        const versionId = version.id === NoVersion.id ? null : version.id;
        IssueApiClient.create(title, description, versionId).then(responseData => {
            navigate(`/issue/${responseData.id}`);
        }).catch(error => {
            console.error(error);
        });
    }

    return (
        <div style={{ display: "flex", justifyContent: "center" }}>
            <AuroraTitle title="Create Issue" />
            <div style={{ width: "50%", backgroundColor: "lightgray", padding: "10px" }}>
                <div><strong>Title</strong></div>
                <div style={{ paddingRight: "10px" }}>
                    <input type="text" value={title} onInput={handleTitleInput} style={{ width: "100%" }} />
                </div>
                <br />
                <div><strong>Description</strong></div>
                <div style={{ paddingRight: "10px" }}>
                    <textarea value={description} onInput={handleDescriptionInput} style={{ width: "100%", height: "500px", resize: "none" }} />
                </div>
                <div style={{ marginTop: "10px" }}>
                    <div><strong>Version</strong></div>
                    <VersionSelect version={version} onVersionSelected={setVersion} />
                </div>
                <div style={{ marginTop: "20px", float: "right" }}>
                    <button onClick={handleCreateButtonClick}>Create</button>
                </div>
            </div>
        </div>
    );
}