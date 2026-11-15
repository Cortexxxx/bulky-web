import {getGuides} from "../api/guide-api.js";
import {withErrorToasts} from "../shared/ui.js";
import {searchByTitle} from "../shared/search.js";

const guideCategories = document.getElementById('guide-categories');
const guideCategoryTemplate = document.getElementById('guide-category-btn-template');
const guideList = document.getElementById('guide-list');
const guideSearch = document.getElementById('guide-search');
const guideCount = document.getElementById('guide-count');
const guideItemTemplate = document.getElementById('guide-item-template');
const guideDialog = document.getElementById('guide-dialog');
const guideTitle = document.getElementById('guide-title');
const guideDescription = document.getElementById('guide-description');
const guideImage = document.getElementById('guide-image');

const categoryLabels = {
    exercise: 'Exercises',
    muscleGroup: 'Muscle groups'
};
const guidesByCategory = new Map(
    Object.keys(categoryLabels).map(category => [category, new Map()])
);
let currentCategory = 'exercise';
let latestRequestId = 0;

setupGuides();

function setupGuides() {
    for (const category of guidesByCategory.keys()) {
        const button = guideCategoryTemplate.content.firstElementChild.cloneNode(true);
        button.id = `guide-button-${category}`;
        button.dataset.category = category;
        button.textContent = categoryLabels[category];
        button.setAttribute('aria-pressed', 'false');
        button.addEventListener('click', e => openCategory(e.currentTarget.dataset.category));
        guideCategories.append(button);
    }

    guideSearch.addEventListener('input', renderGuideList);
    openCategory(currentCategory);
}

async function openCategory(category) {
    const requestId = ++latestRequestId;
    currentCategory = category;
    guideSearch.value = '';
    guidesByCategory.get(category).clear();
    renderGuideList();

    for (const button of guideCategories.querySelectorAll('button[data-category]')) {
        button.setAttribute('aria-pressed', String(button.dataset.category === currentCategory));
    }

    const guides = await withErrorToasts(() => getGuides(category));
    if (requestId !== latestRequestId || !guides) return;

    const items = new Map(guides.map(guide => [guide.sourceId, guide]));
    guidesByCategory.set(category, items);
    renderGuideList();
}

function renderGuideList() {
    const items = guidesByCategory.get(currentCategory);
    const matchingGuides = searchByTitle(items.values(), guideSearch.value);
    const results = document.createDocumentFragment();

    for (const guide of matchingGuides) {
        const item = guideItemTemplate.content.firstElementChild.cloneNode(true);
        const button = item.querySelector('button');
        button.dataset.sourceId = guide.sourceId;
        button.textContent = guide.title;
        button.addEventListener('click', e => {
            const selectedGuide = guidesByCategory.get(currentCategory).get(e.currentTarget.dataset.sourceId);
            showGuide(selectedGuide);
        });
        results.append(item);
    }

    guideList.replaceChildren(results);
    guideCount.textContent = String(matchingGuides.length);
}

function showGuide(guide) {
    guideTitle.textContent = guide.title;
    guideDescription.textContent = guide.description;
    guideImage.hidden = !guide.imagePath;
    if (guide.imagePath) {
        guideImage.src = guide.imagePath;
    } else {
        guideImage.removeAttribute('src');
    }

    guideDialog.showModal();
}
