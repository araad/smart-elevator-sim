const PROXY_CONFIG = {
    "/elevator-tracking": {
        target: "http://scheduling-service:5100",
        secure: false,
        ws: true
    },
    "/api/call-panel": {
        target: "http://call-panel-service:5102",
        secure: false,
    },
    "/api": {
        target: "http://scheduling-service:5100",
        secure: false,
    }
}

module.exports = PROXY_CONFIG;