const PROXY_CONFIG = {
    "/elevator-tracking": {
        target: "http://localhost:5000",
        secure: false,
        ws: true
    },
    "/api/call-panel": {
        target: "http://localhost:5002",
        secure: false,
    },
    "/api": {
        target: "http://localhost:5000",
        secure: false,
    }
}

module.exports = PROXY_CONFIG;