var TabControl =
{
    activePage: function(active)
    {
        var tabControl = active.parents('.tab-control');
        tabControl.tabs = tabControl.find(".tabs a");
        tabControl.tabs.removeClass('active');
        tabControl.pages = tabControl.find(".pages .page");
        tabControl.pages.removeClass('active');
        active.addClass('active');
        var tabIndex = tabControl.tabs.index(active);
        return $(tabControl.pages.get(tabIndex)).addClass('active');
    },
    
    register: function(tabControls)
    {
        tabControls.tabs = tabControls.find(".tabs a");
        tabControls.tabs.attr("href", "javascript:void(0)");
        return tabControls.tabs.click(function()
        {
            return TabControl.activePage($(this));
        });
    }
}

$(function()
{
    TabControl.register($('.tab-control'));
    $('#article-category').change(function()
    {
        $.cookie('articleCategoryId', $(this).val());
        return window.location.reload();
    });
    $('.del').click(function()
    {
        return confirm('Naozaj chcete zmazať túto položku ?');
    });
});