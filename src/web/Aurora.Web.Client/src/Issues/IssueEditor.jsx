import { VersionSelect } from "./VersionSelect";

export function IssueEditor({ data, setData }) {
    function handleTitleInput(event) {
        setData({
            ...data,
            title: event.target.value
        });
    }

    function handleDescriptionInput(event) {
        setData({
            ...data,
            description: event.target.value
        });
    }

    function handleStatusInput(event) {
        setData({
            ...data,
            status: event.target.value
        });
    }

    function handleVersionSelected(version) {
        setData({
            ...data,
            version: version
        });
    }

    return (
        <div>
            <div><strong>Title</strong></div>
            <div style={{ paddingRight: "10px" }}>
                <input type="text" value={data.title} onInput={handleTitleInput} style={{ width: "100%" }} />
            </div>
            <br />
            <div><strong>Description</strong></div>
            <div style={{ paddingRight: "10px" }}>
                <textarea value={data.description} onInput={handleDescriptionInput} style={{ width: "100%", height: "500px", resize: "none" }} />
            </div>
            <div><strong>Status</strong></div>
            <div style={{ paddingRight: "10px" }}>
                <select value={data.status} onInput={handleStatusInput} style={{ width: "200px" }}>
                    <option value="Open">Open</option>
                    <option value="In Progress">In Progress</option>
                    <option value="Closed">Closed</option>
                </select>
            </div>
            <div><strong>Version</strong></div>
            <VersionSelect version={data.version} onVersionSelected={handleVersionSelected} />
        </div>
    );
}
