for (const a of document.querySelectorAll("a[href^='/dashboard']")) {
    if (a.pathname === location.pathname) {
        a.setAttribute("aria-current", "page");
    }
}