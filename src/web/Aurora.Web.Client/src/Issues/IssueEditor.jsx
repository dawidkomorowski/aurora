
export function IssueEditor({ data, setData }) {
    function handleTitleChange(event) {
        setData({
            ...data,
            title: event.target.value
        });
    }

    function handleDescriptionChange(event) {
        setData({
            ...data,
            description: event.target.value
        });
    }

    function handleStatusChange(event) {
        setData({
            ...data,
            status: event.target.value
        });
    }

    return (
        <div>
            <div><strong>Title</strong></div>
            <div style={{ paddingRight: "10px" }}>
                <input type="text" value={data.title} onChange={handleTitleChange} style={{ width: "100%" }} />
            </div>
            <br />
            <div><strong>Description</strong></div>
            <div style={{ paddingRight: "10px" }}>
                <textarea value={data.description} onChange={handleDescriptionChange} style={{ width: "100%", height: "500px", resize: "none" }} />
            </div>
            <div><strong>Status</strong></div>
            <div style={{ paddingRight: "10px" }}>
                <select value={data.status} onChange={handleStatusChange} style={{ width: "200px" }}>
                    <option value="Open">Open</option>
                    <option value="In Progress">In Progress</option>
                    <option value="Closed">Closed</option>
                </select>
            </div>
        </div>
    );
}
