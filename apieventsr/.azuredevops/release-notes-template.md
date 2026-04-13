# 🚀 Release v{{buildDetails.buildNumber}} - {{date_formatter buildDetails.finishTime}}

## 🌵 Associated Pull Requests ({{pullRequests.length}})

{{#forEach pullRequests}}
* !{{pullRequestId}} created by **{{createdBy.displayName}}**
{{/forEach}}

## 🔧 Associated Azure Board Items ({{relatedWorkItems.length}})
{{#forEach this.relatedWorkItems}}
*  Item [#{{this.id}} {{lookup this.fields 'System.Title'}}]({{replace this.url "_apis/wit/workItems" "_workitems/edit"}})
{{/forEach}}

* #### 🐞 Bugs
    {{#forEach workItems}}
    {{#with fields}}
    {{#if (eq (get 'System.WorkItemType' this) 'Bug')}}
    * #{{../id}}  
    {{/if}}
    {{#if (eq (get 'System.WorkItemType' this) 'Incidente')}}
    * #{{../id}}  
    {{/if}}
    {{/with}}
    {{/forEach}}

* #### 🆕 Features
    {{#forEach workItems}}
    {{#with fields}}
    {{#if (eq (get 'System.WorkItemType' this) 'Epic')}}
    * #{{../id}}  
    {{/if}}
    {{#if (eq (get 'System.WorkItemType' this) 'Feature')}}
    * #{{../id}}  
    {{/if}}
    {{#if (eq (get 'System.WorkItemType' this) 'User Story')}}
    * #{{../id}}  
    {{/if}}
    {{#if (eq (get 'System.WorkItemType' this) 'Débito Técnico')}}
    * #{{../id}}  
    {{/if}}
    {{#if (eq (get 'System.WorkItemType' this) 'Melhoria')}}
    * #{{../id}}  
    {{/if}}
    {{/with}}
    {{/forEach}}
---