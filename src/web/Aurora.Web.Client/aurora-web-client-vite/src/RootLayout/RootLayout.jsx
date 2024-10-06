import { Link, Outlet } from "react-router-dom";
import styles from "./RootLayout.module.css"

export function RootLayout() {
    return (
        <>
            <div style={{ backgroundColor: "gray", display: "flex", alignItems: "center", fontWeight: "bold", fontSize: "20px" }}>
                <div style={{ marginLeft: "20px", marginRight: "20px" }}>Aurora</div>
                <nav>
                    <ul style={{ listStyleType: "none", margin: "0px", padding: "0px" }}>
                        <NavigationBarItem title="Issue Explorer" to="/issue-explorer" />
                        <NavigationBarItem title="Create Issue" to="/issue/create" />
                    </ul>
                </nav>
            </div>
            <div>
                <Outlet />
            </div>
        </>
    );
}

function NavigationBarItem({ title, to }) {
    return (
        <li style={{ float: "left" }}>
            <Link className={styles.navLink} to={to}>{title}</Link>
        </li>
    );
}