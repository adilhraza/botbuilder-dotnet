﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Dialogs.Composition.Recognizers
{

    /// <summary>
    /// EntityRecognizerSet - Implements a workflow against a pool of IEntityRecognizer instances, iterating until nobody has anything new to add.
    /// </summary>
    public class EntityRecognizerSet : IEntityRecognizer
    {
        public EntityRecognizerSet() { }

        /// <summary>
        /// Recognizer pool 
        /// </summary>
        public IList<IEntityRecognizer> Recognizers { get; set; } = new List<IEntityRecognizer>();

        /// <summary>
        /// Implement RecognizeEntities by iterating against the Recognizer pool.
        /// </summary>
        /// <param name="turnContext"></param>
        /// <param name="entities"></param>
        /// <returns></returns>
        public async Task<IList<Entity>> RecognizeEntities(ITurnContext turnContext, IList<Entity> entities = null)
        {
            List<Entity> allNewEntities = new List<Entity>();
            List<Entity> entitiesToProcess = new List<Entity>(entities ?? Array.Empty<Entity>());

            do
            {
                List<Entity> newEntitiesToProcess = new List<Entity>();

                foreach (var recognizer in this.Recognizers)
                {
                    try
                    {

                        // get new entities
                        var newEntities = await recognizer.RecognizeEntities(turnContext, entitiesToProcess).ConfigureAwait(false);

                        foreach (var newEntity in newEntities)
                        {
                            // if unique
                            if (!allNewEntities.Any(entity => entity.Equals(newEntity)))
                            {
                                // add to all results
                                allNewEntities.Add(newEntity);

                                // add to list to be processed more
                                newEntitiesToProcess.Add(newEntity);
                            }
                        }
                    }
                    catch (Exception err)
                    {
                        System.Diagnostics.Trace.TraceWarning(err.Message);
                    }
                }

                // switch to next pool of new entities to process
                entitiesToProcess = newEntitiesToProcess;

            } while (entitiesToProcess.Count > 0);

            return allNewEntities;
        }
    }
}