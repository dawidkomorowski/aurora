import { AuroraTitle } from "../Components/AuroraTitle";

export function NotFoundView() {
    return (
        <div>
            <AuroraTitle title="Page not found" />
            <div style={{ width: "100%", display: "flex", justifyContent: "center", fontSize: "128px" }}>
                <strong>404</strong>
            </div>
        </div>
    );
}
