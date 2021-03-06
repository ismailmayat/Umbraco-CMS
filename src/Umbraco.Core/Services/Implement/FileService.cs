﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Umbraco.Core.Events;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Persistence.Repositories.Implement;
using Umbraco.Core.Scoping;

namespace Umbraco.Core.Services.Implement
{
    /// <summary>
    /// Represents the File Service, which is an easy access to operations involving <see cref="IFile"/> objects like Scripts, Stylesheets and Templates
    /// </summary>
    public class FileService : ScopeRepositoryService, IFileService
    {
        private readonly IStylesheetRepository _stylesheetRepository;
        private readonly IScriptRepository _scriptRepository;
        private readonly ITemplateRepository _templateRepository;
        private readonly IPartialViewRepository _partialViewRepository;
        private readonly IPartialViewMacroRepository _partialViewMacroRepository;
        private readonly IAuditRepository _auditRepository;

        private const string PartialViewHeader = "@inherits Umbraco.Web.Mvc.UmbracoViewPage";
        private const string PartialViewMacroHeader = "@inherits Umbraco.Web.Macros.PartialViewMacroPage";

        public FileService(IScopeProvider uowProvider, ILogger logger, IEventMessagesFactory eventMessagesFactory,
            IStylesheetRepository stylesheetRepository, IScriptRepository scriptRepository, ITemplateRepository templateRepository,
            IPartialViewRepository partialViewRepository, IPartialViewMacroRepository partialViewMacroRepository,
            IAuditRepository auditRepository)
            : base(uowProvider, logger, eventMessagesFactory)
        {
            _stylesheetRepository = stylesheetRepository;
            _scriptRepository = scriptRepository;
            _templateRepository = templateRepository;
            _partialViewRepository = partialViewRepository;
            _partialViewMacroRepository = partialViewMacroRepository;
            _auditRepository = auditRepository;
        }

        #region Stylesheets

        /// <summary>
        /// Gets a list of all <see cref="Stylesheet"/> objects
        /// </summary>
        /// <returns>An enumerable list of <see cref="Stylesheet"/> objects</returns>
        public IEnumerable<Stylesheet> GetStylesheets(params string[] names)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                return _stylesheetRepository.GetMany(names);
            }
        }

        /// <summary>
        /// Gets a <see cref="Stylesheet"/> object by its name
        /// </summary>
        /// <param name="name">Name of the stylesheet incl. extension</param>
        /// <returns>A <see cref="Stylesheet"/> object</returns>
        public Stylesheet GetStylesheetByName(string name)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                return _stylesheetRepository.Get(name);
            }
        }

        /// <summary>
        /// Saves a <see cref="Stylesheet"/>
        /// </summary>
        /// <param name="stylesheet"><see cref="Stylesheet"/> to save</param>
        /// <param name="userId"></param>
        public void SaveStylesheet(Stylesheet stylesheet, int userId = 0)
        {
            using (var scope = ScopeProvider.CreateScope())
            {
                var saveEventArgs = new SaveEventArgs<Stylesheet>(stylesheet);
                if (scope.Events.DispatchCancelable(SavingStylesheet, this, saveEventArgs))
                {
                    scope.Complete();
                    return;
                }


                _stylesheetRepository.Save(stylesheet);
                saveEventArgs.CanCancel = false;
                scope.Events.Dispatch(SavedStylesheet, this, saveEventArgs);

                Audit(AuditType.Save, "Save Stylesheet performed by user", userId, -1);
                scope.Complete();
            }
        }

        /// <summary>
        /// Deletes a stylesheet by its name
        /// </summary>
        /// <param name="path">Name incl. extension of the Stylesheet to delete</param>
        /// <param name="userId"></param>
        public void DeleteStylesheet(string path, int userId = 0)
        {
            using (var scope = ScopeProvider.CreateScope())
            {
                var stylesheet = _stylesheetRepository.Get(path);
                if (stylesheet == null)
                {
                    scope.Complete();
                    return;
                }

                var deleteEventArgs = new DeleteEventArgs<Stylesheet>(stylesheet);
                if (scope.Events.DispatchCancelable(DeletingStylesheet, this, deleteEventArgs))
                {
                    scope.Complete();
                    return; // causes rollback // causes rollback
                }

                _stylesheetRepository.Delete(stylesheet);
                deleteEventArgs.CanCancel = false;
                scope.Events.Dispatch(DeletedStylesheet, this, deleteEventArgs);

                Audit(AuditType.Delete, "Delete Stylesheet performed by user", userId, -1);
                scope.Complete();
            }
        }

        /// <summary>
        /// Validates a <see cref="Stylesheet"/>
        /// </summary>
        /// <param name="stylesheet"><see cref="Stylesheet"/> to validate</param>
        /// <returns>True if Stylesheet is valid, otherwise false</returns>
        public bool ValidateStylesheet(Stylesheet stylesheet)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                return _stylesheetRepository.ValidateStylesheet(stylesheet);
            }
        }

        public Stream GetStylesheetFileContentStream(string filepath)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                return _stylesheetRepository.GetFileContentStream(filepath);
            }
        }

        public void SetStylesheetFileContent(string filepath, Stream content)
        {
            using (var scope = ScopeProvider.CreateScope())
            {
                _stylesheetRepository.SetFileContent(filepath, content);
                scope.Complete();
            }
        }

        public long GetStylesheetFileSize(string filepath)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                return _stylesheetRepository.GetFileSize(filepath);
            }
        }

        #endregion

        #region Scripts

        /// <summary>
        /// Gets a list of all <see cref="Script"/> objects
        /// </summary>
        /// <returns>An enumerable list of <see cref="Script"/> objects</returns>
        public IEnumerable<Script> GetScripts(params string[] names)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                return _scriptRepository.GetMany(names);
            }
        }

        /// <summary>
        /// Gets a <see cref="Script"/> object by its name
        /// </summary>
        /// <param name="name">Name of the script incl. extension</param>
        /// <returns>A <see cref="Script"/> object</returns>
        public Script GetScriptByName(string name)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                return _scriptRepository.Get(name);
            }
        }

        /// <summary>
        /// Saves a <see cref="Script"/>
        /// </summary>
        /// <param name="script"><see cref="Script"/> to save</param>
        /// <param name="userId"></param>
        public void SaveScript(Script script, int userId = 0)
        {
            using (var scope = ScopeProvider.CreateScope())
            {
                var saveEventArgs = new SaveEventArgs<Script>(script);
                if (scope.Events.DispatchCancelable(SavingScript, this, saveEventArgs))
                {
                    scope.Complete();
                    return;
                }

                _scriptRepository.Save(script);
                saveEventArgs.CanCancel = false;
                scope.Events.Dispatch(SavedScript, this, saveEventArgs);

                Audit(AuditType.Save, "Save Script performed by user", userId, -1);
                scope.Complete();
            }
        }

        /// <summary>
        /// Deletes a script by its name
        /// </summary>
        /// <param name="path">Name incl. extension of the Script to delete</param>
        /// <param name="userId"></param>
        public void DeleteScript(string path, int userId = 0)
        {
            using (var scope = ScopeProvider.CreateScope())
            {
                var script = _scriptRepository.Get(path);
                if (script == null)
                {
                    scope.Complete();
                    return;
                }

                var deleteEventArgs = new DeleteEventArgs<Script>(script);
                if (scope.Events.DispatchCancelable(DeletingScript, this, deleteEventArgs))
                {
                    scope.Complete();
                    return;
                }

                _scriptRepository.Delete(script);
                deleteEventArgs.CanCancel = false;
                scope.Events.Dispatch(DeletedScript, this, deleteEventArgs);

                Audit(AuditType.Delete, "Delete Script performed by user", userId, -1);
                scope.Complete();
            }
        }

        /// <summary>
        /// Validates a <see cref="Script"/>
        /// </summary>
        /// <param name="script"><see cref="Script"/> to validate</param>
        /// <returns>True if Script is valid, otherwise false</returns>
        public bool ValidateScript(Script script)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                return _scriptRepository.ValidateScript(script);
            }
        }

        public void CreateScriptFolder(string folderPath)
        {
            using (var scope = ScopeProvider.CreateScope())
            {
                ((ScriptRepository) _scriptRepository).AddFolder(folderPath);
                scope.Complete();
            }
        }

        public void DeleteScriptFolder(string folderPath)
        {
            using (var scope = ScopeProvider.CreateScope())
            {
                ((ScriptRepository) _scriptRepository).DeleteFolder(folderPath);
                scope.Complete();
            }
        }

        public Stream GetScriptFileContentStream(string filepath)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                return _scriptRepository.GetFileContentStream(filepath);
            }
        }

        public void SetScriptFileContent(string filepath, Stream content)
        {
            using (var scope = ScopeProvider.CreateScope())
            {
                _scriptRepository.SetFileContent(filepath, content);
                scope.Complete();
            }
        }

        public long GetScriptFileSize(string filepath)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                return _scriptRepository.GetFileSize(filepath);
            }
        }

        #endregion

        #region Templates

        /// <summary>
        /// Creates a template for a content type
        /// </summary>
        /// <param name="contentTypeAlias"></param>
        /// <param name="contentTypeName"></param>
        /// <param name="userId"></param>
        /// <returns>
        /// The template created
        /// </returns>
        public Attempt<OperationResult<OperationResultType, ITemplate>> CreateTemplateForContentType(string contentTypeAlias, string contentTypeName, int userId = 0)
        {
            var template = new Template(contentTypeName,
                //NOTE: We are NOT passing in the content type alias here, we want to use it's name since we don't
                // want to save template file names as camelCase, the Template ctor will clean the alias as
                // `alias.ToCleanString(CleanStringType.UnderscoreAlias)` which has been the default.
                // This fixes: http://issues.umbraco.org/issue/U4-7953
                contentTypeName);

            var evtMsgs = EventMessagesFactory.Get();

            //NOTE: This isn't pretty but we need to maintain backwards compatibility so we cannot change
            // the event args here. The other option is to create a different event with different event
            // args specifically for this method... which also isn't pretty. So for now, we'll use this
            // dictionary approach to store 'additional data' in.
            var additionalData = new Dictionary<string, object>
            {
                { "CreateTemplateForContentType", true },
                { "ContentTypeAlias", contentTypeAlias },
            };

            using (var scope = ScopeProvider.CreateScope())
            {
                var saveEventArgs = new SaveEventArgs<ITemplate>(template, true, evtMsgs, additionalData);
                if (scope.Events.DispatchCancelable(SavingTemplate, this, saveEventArgs))
                {
                    scope.Complete();
                    return OperationResult.Attempt.Fail<OperationResultType, ITemplate>(OperationResultType.FailedCancelledByEvent, evtMsgs, template);
                }

                _templateRepository.Save(template);
                saveEventArgs.CanCancel = false;
                scope.Events.Dispatch(SavedTemplate, this, saveEventArgs);

                Audit(AuditType.Save, "Save Template performed by user", userId, template.Id);
                scope.Complete();
            }

            return OperationResult.Attempt.Succeed<OperationResultType, ITemplate>(OperationResultType.Success, evtMsgs, template);
        }

        public ITemplate CreateTemplateWithIdentity(string name, string content, ITemplate masterTemplate = null, int userId = 0)
        {
            var template = new Template(name, name)
            {
                Content = content
            };
            if (masterTemplate != null)
            {
                template.SetMasterTemplate(masterTemplate);
            }
            SaveTemplate(template, userId);
            return template;
        }

        /// <summary>
        /// Gets a list of all <see cref="ITemplate"/> objects
        /// </summary>
        /// <returns>An enumerable list of <see cref="ITemplate"/> objects</returns>
        public IEnumerable<ITemplate> GetTemplates(params string[] aliases)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                return _templateRepository.GetAll(aliases).OrderBy(x => x.Name);
            }
        }

        /// <summary>
        /// Gets a list of all <see cref="ITemplate"/> objects
        /// </summary>
        /// <returns>An enumerable list of <see cref="ITemplate"/> objects</returns>
        public IEnumerable<ITemplate> GetTemplates(int masterTemplateId)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                return _templateRepository.GetChildren(masterTemplateId).OrderBy(x => x.Name);
            }
        }

        /// <summary>
        /// Gets a <see cref="ITemplate"/> object by its alias.
        /// </summary>
        /// <param name="alias">The alias of the template.</param>
        /// <returns>The <see cref="ITemplate"/> object matching the alias, or null.</returns>
        public ITemplate GetTemplate(string alias)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                return _templateRepository.Get(alias);
            }
        }

        /// <summary>
        /// Gets a <see cref="ITemplate"/> object by its identifier.
        /// </summary>
        /// <param name="id">The identifer of the template.</param>
        /// <returns>The <see cref="ITemplate"/> object matching the identifier, or null.</returns>
        public ITemplate GetTemplate(int id)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                return _templateRepository.Get(id);
            }
        }

        /// <summary>
        /// Gets a <see cref="ITemplate"/> object by its guid identifier.
        /// </summary>
        /// <param name="id">The guid identifier of the template.</param>
        /// <returns>The <see cref="ITemplate"/> object matching the identifier, or null.</returns>
        public ITemplate GetTemplate(Guid id)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                var query = Query<ITemplate>().Where(x => x.Key == id);
                return _templateRepository.Get(query).SingleOrDefault();
            }
        }

        public IEnumerable<ITemplate> GetTemplateDescendants(string alias)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                return _templateRepository.GetDescendants(alias);
            }
        }

        /// <summary>
        /// Gets the template descendants
        /// </summary>
        /// <param name="masterTemplateId"></param>
        /// <returns></returns>
        public IEnumerable<ITemplate> GetTemplateDescendants(int masterTemplateId)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                return _templateRepository.GetDescendants(masterTemplateId);
            }
        }

        /// <summary>
        /// Gets the template children
        /// </summary>
        /// <param name="alias"></param>
        /// <returns></returns>
        public IEnumerable<ITemplate> GetTemplateChildren(string alias)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                return _templateRepository.GetChildren(alias);
            }
        }

        /// <summary>
        /// Gets the template children
        /// </summary>
        /// <param name="masterTemplateId"></param>
        /// <returns></returns>
        public IEnumerable<ITemplate> GetTemplateChildren(int masterTemplateId)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                return _templateRepository.GetChildren(masterTemplateId);
            }
        }

        /// <summary>
        /// Returns a template as a template node which can be traversed (parent, children)
        /// </summary>
        /// <param name="alias"></param>
        /// <returns></returns>
        [Obsolete("Use GetDescendants instead")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public TemplateNode GetTemplateNode(string alias)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                return _templateRepository.GetTemplateNode(alias);
            }
        }

        /// <summary>
        /// Given a template node in a tree, this will find the template node with the given alias if it is found in the hierarchy, otherwise null
        /// </summary>
        /// <param name="anyNode"></param>
        /// <param name="alias"></param>
        /// <returns></returns>
        [Obsolete("Use GetDescendants instead")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public TemplateNode FindTemplateInTree(TemplateNode anyNode, string alias)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                return _templateRepository.FindTemplateInTree(anyNode, alias);
            }
        }

        /// <summary>
        /// Saves a <see cref="Template"/>
        /// </summary>
        /// <param name="template"><see cref="Template"/> to save</param>
        /// <param name="userId"></param>
        public void SaveTemplate(ITemplate template, int userId = 0)
        {
            using (var scope = ScopeProvider.CreateScope())
            {
                if (scope.Events.DispatchCancelable(SavingTemplate, this, new SaveEventArgs<ITemplate>(template)))
                {
                    scope.Complete();
                    return;
                }

                _templateRepository.Save(template);

                scope.Events.Dispatch(SavedTemplate, this, new SaveEventArgs<ITemplate>(template, false));

                Audit(AuditType.Save, "Save Template performed by user", userId, template.Id);
                scope.Complete();
            }
        }

        /// <summary>
        /// Saves a collection of <see cref="Template"/> objects
        /// </summary>
        /// <param name="templates">List of <see cref="Template"/> to save</param>
        /// <param name="userId">Optional id of the user</param>
        public void SaveTemplate(IEnumerable<ITemplate> templates, int userId = 0)
        {
            var templatesA = templates.ToArray();
            using (var scope = ScopeProvider.CreateScope())
            {
                if (scope.Events.DispatchCancelable(SavingTemplate, this, new SaveEventArgs<ITemplate>(templatesA)))
                {
                    scope.Complete();
                    return;
                }

                foreach (var template in templatesA)
                    _templateRepository.Save(template);

                scope.Events.Dispatch(SavedTemplate, this, new SaveEventArgs<ITemplate>(templatesA, false));

                Audit(AuditType.Save, "Save Template performed by user", userId, -1);
                scope.Complete();
            }
        }

        /// <summary>
        /// This checks what the default rendering engine is set in config but then also ensures that there isn't already
        /// a template that exists in the opposite rendering engine's template folder, then returns the appropriate
        /// rendering engine to use.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// The reason this is required is because for example, if you have a master page file already existing under ~/masterpages/Blah.aspx
        /// and then you go to create a template in the tree called Blah and the default rendering engine is MVC, it will create a Blah.cshtml
        /// empty template in ~/Views. This means every page that is using Blah will go to MVC and render an empty page.
        /// This is mostly related to installing packages since packages install file templates to the file system and then create the
        /// templates in business logic. Without this, it could cause the wrong rendering engine to be used for a package.
        /// </remarks>
        public RenderingEngine DetermineTemplateRenderingEngine(ITemplate template)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                return _templateRepository.DetermineTemplateRenderingEngine(template);
            }
        }

        /// <summary>
        /// Deletes a template by its alias
        /// </summary>
        /// <param name="alias">Alias of the <see cref="ITemplate"/> to delete</param>
        /// <param name="userId"></param>
        public void DeleteTemplate(string alias, int userId = 0)
        {
            using (var scope = ScopeProvider.CreateScope())
            {
                var template = _templateRepository.Get(alias);
                if (template == null)
                {
                    scope.Complete();
                    return;
                }

                var args = new DeleteEventArgs<ITemplate>(template);
                if (scope.Events.DispatchCancelable(DeletingTemplate, this, args))
                {
                    scope.Complete();
                    return;
                }

                _templateRepository.Delete(template);

                args.CanCancel = false;
                scope.Events.Dispatch(DeletedTemplate, this, args);

                Audit(AuditType.Delete, "Delete Template performed by user", userId, template.Id);
                scope.Complete();
            }
        }

        /// <summary>
        /// Validates a <see cref="ITemplate"/>
        /// </summary>
        /// <param name="template"><see cref="ITemplate"/> to validate</param>
        /// <returns>True if Script is valid, otherwise false</returns>
        public bool ValidateTemplate(ITemplate template)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                return _templateRepository.ValidateTemplate(template);
            }
        }

        public Stream GetTemplateFileContentStream(string filepath)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                return _templateRepository.GetFileContentStream(filepath);
            }
        }

        public void SetTemplateFileContent(string filepath, Stream content)
        {
            using (var scope = ScopeProvider.CreateScope())
            {
                _templateRepository.SetFileContent(filepath, content);
                scope.Complete();
            }
        }

        public long GetTemplateFileSize(string filepath)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                return _templateRepository.GetFileSize(filepath);
            }
        }

        #endregion

        #region Partial Views

        public IEnumerable<string> GetPartialViewSnippetNames(params string[] filterNames)
        {
            var snippetPath = IOHelper.MapPath($"{SystemDirectories.Umbraco}/PartialViewMacros/Templates/");
            var files = Directory.GetFiles(snippetPath, "*.cshtml")
                .Select(Path.GetFileNameWithoutExtension)
                .Except(filterNames, StringComparer.InvariantCultureIgnoreCase)
                .ToArray();

            //Ensure the ones that are called 'Empty' are at the top
            var empty = files.Where(x => Path.GetFileName(x).InvariantStartsWith("Empty"))
                .OrderBy(x => x.Length)
                .ToArray();

            return empty.Union(files.Except(empty));
        }

        public void DeletePartialViewFolder(string folderPath)
        {
            using (var scope = ScopeProvider.CreateScope())
            {
                _partialViewRepository.DeleteFolder(folderPath);
                scope.Complete();
            }
        }

        public void DeletePartialViewMacroFolder(string folderPath)
        {
            using (var scope = ScopeProvider.CreateScope())
            {
                _partialViewMacroRepository.DeleteFolder(folderPath);
                scope.Complete();
            }
        }

        public IPartialView GetPartialView(string path)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                return _partialViewRepository.Get(path);
            }
        }

        public IPartialView GetPartialViewMacro(string path)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                return _partialViewMacroRepository.Get(path);
            }
        }

        public IEnumerable<IPartialView> GetPartialViewMacros(params string[] names)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                return _partialViewMacroRepository.GetMany(names).OrderBy(x => x.Name);
            }
        }

        public Attempt<IPartialView> CreatePartialView(IPartialView partialView, string snippetName = null, int userId = 0)
        {
            return CreatePartialViewMacro(partialView, PartialViewType.PartialView, snippetName, userId);
        }

        public Attempt<IPartialView> CreatePartialViewMacro(IPartialView partialView, string snippetName = null, int userId = 0)
        {
            return CreatePartialViewMacro(partialView, PartialViewType.PartialViewMacro, snippetName, userId);
        }

        private Attempt<IPartialView> CreatePartialViewMacro(IPartialView partialView, PartialViewType partialViewType, string snippetName = null, int userId = 0)
        {
            string partialViewHeader;
            switch (partialViewType)
            {
                case PartialViewType.PartialView:
                    partialViewHeader = PartialViewHeader;
                    break;
                case PartialViewType.PartialViewMacro:
                    partialViewHeader = PartialViewMacroHeader;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(partialViewType));
            }

            string partialViewContent = null;
            if (snippetName.IsNullOrWhiteSpace() == false)
            {
                //create the file
                var snippetPathAttempt = TryGetSnippetPath(snippetName);
                if (snippetPathAttempt.Success == false)
                {
                    throw new InvalidOperationException("Could not load snippet with name " + snippetName);
                }

                using (var snippetFile = new StreamReader(System.IO.File.OpenRead(snippetPathAttempt.Result)))
                {
                    var snippetContent = snippetFile.ReadToEnd().Trim();

                    //strip the @inherits if it's there
                    snippetContent = StripPartialViewHeader(snippetContent);

                    partialViewContent = $"{partialViewHeader}{Environment.NewLine}{snippetContent}";
                }
            }

            using (var scope = ScopeProvider.CreateScope())
            {
                var newEventArgs = new NewEventArgs<IPartialView>(partialView, true, partialView.Alias, -1);
                if (scope.Events.DispatchCancelable(CreatingPartialView, this, newEventArgs))
                {
                    scope.Complete();
                    return Attempt<IPartialView>.Fail();
                }

                var repository = GetPartialViewRepository(partialViewType);
                if (partialViewContent != null) partialView.Content = partialViewContent;
                repository.Save(partialView);

                newEventArgs.CanCancel = false;
                scope.Events.Dispatch(CreatedPartialView, this, newEventArgs);

                Audit(AuditType.Save, $"Save {partialViewType} performed by user", userId, -1);

                scope.Complete();
            }

            return Attempt<IPartialView>.Succeed(partialView);
        }

        public bool DeletePartialView(string path, int userId = 0)
        {
            return DeletePartialViewMacro(path, PartialViewType.PartialView, userId);
        }

        public bool DeletePartialViewMacro(string path, int userId = 0)
        {
            return DeletePartialViewMacro(path, PartialViewType.PartialViewMacro, userId);
        }

        private bool DeletePartialViewMacro(string path, PartialViewType partialViewType, int userId = 0)
        {
            using (var scope = ScopeProvider.CreateScope())
            {
                var repository = GetPartialViewRepository(partialViewType);
                var partialView = repository.Get(path);
                if (partialView == null)
                {
                    scope.Complete();
                    return true;
                }

                var deleteEventArgs = new DeleteEventArgs<IPartialView>(partialView);
                if (scope.Events.DispatchCancelable(DeletingPartialView, this, deleteEventArgs))
                {
                    scope.Complete();
                    return false;
                }

                repository.Delete(partialView);
                deleteEventArgs.CanCancel = false;
                scope.Events.Dispatch(DeletedPartialView, this, deleteEventArgs);
                Audit(AuditType.Delete, $"Delete {partialViewType} performed by user", userId, -1);

                scope.Complete();
            }

            return true;
        }

        public Attempt<IPartialView> SavePartialView(IPartialView partialView, int userId = 0)
        {
            return SavePartialView(partialView, PartialViewType.PartialView, userId);
        }

        public Attempt<IPartialView> SavePartialViewMacro(IPartialView partialView, int userId = 0)
        {
            return SavePartialView(partialView, PartialViewType.PartialViewMacro, userId);
        }

        private Attempt<IPartialView> SavePartialView(IPartialView partialView, PartialViewType partialViewType, int userId = 0)
        {
            using (var scope = ScopeProvider.CreateScope())
            {
                var saveEventArgs = new SaveEventArgs<IPartialView>(partialView);
                if (scope.Events.DispatchCancelable(SavingPartialView, this, saveEventArgs))
                {
                    scope.Complete();
                    return Attempt<IPartialView>.Fail();
                }

                var repository = GetPartialViewRepository(partialViewType);
                repository.Save(partialView);
                saveEventArgs.CanCancel = false;
                Audit(AuditType.Save, $"Save {partialViewType} performed by user", userId, -1);
                scope.Events.Dispatch(SavedPartialView, this, saveEventArgs);

                scope.Complete();
            }

            return Attempt.Succeed(partialView);
        }

        public bool ValidatePartialView(PartialView partialView)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                return _partialViewRepository.ValidatePartialView(partialView);
            }
        }

        public bool ValidatePartialViewMacro(PartialView partialView)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                return _partialViewMacroRepository.ValidatePartialView(partialView);
            }
        }

        internal string StripPartialViewHeader(string contents)
        {
            var headerMatch = new Regex("^@inherits\\s+?.*$", RegexOptions.Multiline);
            return headerMatch.Replace(contents, string.Empty);
        }

        internal Attempt<string> TryGetSnippetPath(string fileName)
        {
            if (fileName.EndsWith(".cshtml") == false)
            {
                fileName += ".cshtml";
            }

            var snippetPath = IOHelper.MapPath($"{SystemDirectories.Umbraco}/PartialViewMacros/Templates/{fileName}");
            return System.IO.File.Exists(snippetPath)
                ? Attempt<string>.Succeed(snippetPath)
                : Attempt<string>.Fail();
        }

        public Stream GetPartialViewMacroFileContentStream(string filepath)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                return _partialViewMacroRepository.GetFileContentStream(filepath);
            }
        }

        public void SetPartialViewMacroFileContent(string filepath, Stream content)
        {
            using (var scope = ScopeProvider.CreateScope())
            {
                _partialViewMacroRepository.SetFileContent(filepath, content);
                scope.Complete();
            }
        }

        public Stream GetPartialViewFileContentStream(string filepath)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                return _partialViewRepository.GetFileContentStream(filepath);
            }
        }

        public void SetPartialViewFileContent(string filepath, Stream content)
        {
            using (var scope = ScopeProvider.CreateScope())
            {
                _partialViewRepository.SetFileContent(filepath, content);
                scope.Complete();
            }
        }

        public void CreatePartialViewFolder(string folderPath)
        {
            using (var scope = ScopeProvider.CreateScope())
            {
                _partialViewRepository.AddFolder(folderPath);
                scope.Complete();
            }
        }

        public void CreatePartialViewMacroFolder(string folderPath)
        {
            using (var scope = ScopeProvider.CreateScope())
            {
                _partialViewMacroRepository.AddFolder(folderPath);
                scope.Complete();
            }
        }

        public long GetPartialViewMacroFileSize(string filepath)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                return _partialViewMacroRepository.GetFileSize(filepath);
            }
        }

        public long GetPartialViewFileSize(string filepath)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                return _partialViewRepository.GetFileSize(filepath);
            }
        }

        private IPartialViewRepository GetPartialViewRepository(PartialViewType partialViewType)
        {
            switch (partialViewType)
            {
                case PartialViewType.PartialView:
                    return _partialViewRepository;
                case PartialViewType.PartialViewMacro:
                    return _partialViewMacroRepository;
                default:
                    throw new ArgumentOutOfRangeException(nameof(partialViewType));
            }
        }

        #endregion

        #region Snippets

        public string GetPartialViewSnippetContent(string snippetName)
        {
            return GetPartialViewMacroSnippetContent(snippetName, PartialViewType.PartialView);
        }

        public string GetPartialViewMacroSnippetContent(string snippetName)
        {
            return GetPartialViewMacroSnippetContent(snippetName, PartialViewType.PartialViewMacro);
        }

        private string GetPartialViewMacroSnippetContent(string snippetName, PartialViewType partialViewType)
        {
            if (snippetName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(snippetName));

            string partialViewHeader;
            switch (partialViewType)
            {
                case PartialViewType.PartialView:
                    partialViewHeader = PartialViewHeader;
                    break;
                case PartialViewType.PartialViewMacro:
                    partialViewHeader = PartialViewMacroHeader;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(partialViewType));
            }

            // Try and get the snippet path
            var snippetPathAttempt = TryGetSnippetPath(snippetName);
            if (snippetPathAttempt.Success == false)
            {
                throw new InvalidOperationException("Could not load snippet with name " + snippetName);
            }

            using (var snippetFile = new StreamReader(System.IO.File.OpenRead(snippetPathAttempt.Result)))
            {
                var snippetContent = snippetFile.ReadToEnd().Trim();

                //strip the @inherits if it's there
                snippetContent = StripPartialViewHeader(snippetContent);

                var content = $"{partialViewHeader}{Environment.NewLine}{snippetContent}";
                return content;
            }
        }

        #endregion
        
        private void Audit(AuditType type, string message, int userId, int objectId)
        {
            _auditRepository.Save(new AuditItem(objectId, message, type, userId));
        }

        //TODO Method to change name and/or alias of view/masterpage template

        #region Event Handlers

        /// <summary>
        /// Occurs before Delete
        /// </summary>
        public static event TypedEventHandler<IFileService, DeleteEventArgs<ITemplate>> DeletingTemplate;

        /// <summary>
        /// Occurs after Delete
        /// </summary>
        public static event TypedEventHandler<IFileService, DeleteEventArgs<ITemplate>> DeletedTemplate;

        /// <summary>
        /// Occurs before Delete
        /// </summary>
        public static event TypedEventHandler<IFileService, DeleteEventArgs<Script>> DeletingScript;

        /// <summary>
        /// Occurs after Delete
        /// </summary>
        public static event TypedEventHandler<IFileService, DeleteEventArgs<Script>> DeletedScript;

        /// <summary>
        /// Occurs before Delete
        /// </summary>
        public static event TypedEventHandler<IFileService, DeleteEventArgs<Stylesheet>> DeletingStylesheet;

        /// <summary>
        /// Occurs after Delete
        /// </summary>
        public static event TypedEventHandler<IFileService, DeleteEventArgs<Stylesheet>> DeletedStylesheet;

        /// <summary>
        /// Occurs before Save
        /// </summary>
        public static event TypedEventHandler<IFileService, SaveEventArgs<ITemplate>> SavingTemplate;

        /// <summary>
        /// Occurs after Save
        /// </summary>
        public static event TypedEventHandler<IFileService, SaveEventArgs<ITemplate>> SavedTemplate;

        /// <summary>
        /// Occurs before Save
        /// </summary>
        public static event TypedEventHandler<IFileService, SaveEventArgs<Script>> SavingScript;

        /// <summary>
        /// Occurs after Save
        /// </summary>
        public static event TypedEventHandler<IFileService, SaveEventArgs<Script>> SavedScript;

        /// <summary>
        /// Occurs before Save
        /// </summary>
        public static event TypedEventHandler<IFileService, SaveEventArgs<Stylesheet>> SavingStylesheet;

        /// <summary>
        /// Occurs after Save
        /// </summary>
        public static event TypedEventHandler<IFileService, SaveEventArgs<Stylesheet>> SavedStylesheet;

        /// <summary>
        /// Occurs before Save
        /// </summary>
        public static event TypedEventHandler<IFileService, SaveEventArgs<IPartialView>> SavingPartialView;

        /// <summary>
        /// Occurs after Save
        /// </summary>
        public static event TypedEventHandler<IFileService, SaveEventArgs<IPartialView>> SavedPartialView;

        /// <summary>
        /// Occurs before Create
        /// </summary>
        public static event TypedEventHandler<IFileService, NewEventArgs<IPartialView>> CreatingPartialView;

        /// <summary>
        /// Occurs after Create
        /// </summary>
        public static event TypedEventHandler<IFileService, NewEventArgs<IPartialView>> CreatedPartialView;

        /// <summary>
        /// Occurs before Delete
        /// </summary>
        public static event TypedEventHandler<IFileService, DeleteEventArgs<IPartialView>> DeletingPartialView;

        /// <summary>
        /// Occurs after Delete
        /// </summary>
        public static event TypedEventHandler<IFileService, DeleteEventArgs<IPartialView>> DeletedPartialView;

        #endregion
    }
}
